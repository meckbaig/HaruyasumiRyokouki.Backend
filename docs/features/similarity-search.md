# Similarity search

Finds media that look alike by comparing image embeddings. Two admin endpoints use it:
"what is similar to this media" and "what else should get this tag".

## Files

| File | Role |
| --- | --- |
| `Services/MediaSimilarityIndexService.cs` | In-memory index of all vectors, cosine ranking. |
| `Services/BackgroundServices/SimilarityWarmupBackgroundService.cs` | Loads the index on startup. |
| `Features/Media/GetSimilarMediaQuery.cs` | `GET /v1/media/{id}/similar?take=50`. |
| `Features/Tags/SuggestMediaForTagQuery.cs` | `GET /v1/tags/{id}/suggest?take=300`. |
| `Common/HealthChecks/SimilaritySearchHealthCheck.cs` | `similarity-search` entry of `/health`. |
| `Models/Db/MediaEmbedding.cs` | `media_embeddings`: media id, 768-float vector, model name. |

## Where the vectors come from

The backend does not compute embeddings. `media_embeddings` was filled once by an external tool
for the media of the initial import; media added later have no vector and are never found or
used as a query. A media without a vector gives an empty list.

## The index

- On startup all vectors are read into one flat `float[]` (768 per media, contiguous for SIMD) plus an id → row map. 
- The vectors are expected to be normalized, so the dot product is the cosine similarity. `Centroid` normalizes its result.
- If the index is empty when an endpoint is called (warmup failed or the table was empty), it is loaded again on that request.

| Endpoint | Query vector | Excluded |
| --- | --- | --- |
| `/media/{id}/similar` | The vector of the media | The media itself |
| `/tags/{id}/suggest` | Normalized average of the vectors of media that already have the tag | Media that already have the tag |

Both return all candidates ranked by score, cut to `take` - there is no threshold; the admin
decides where the results stop being useful. `/tags/{id}/suggest` returns `seedCount` and **no
items when fewer than 3 media have the tag** - too few to describe the subject.

## Switching it off

`Application:SimilaritySearchEnabled = false` skips loading the vectors (they take memory: about 3 KB per
media) and makes both endpoints return **503**. `/health` reports the check as `Healthy` with
`Disabled.`; enabled with no vectors loaded is `Unhealthy`, which makes the overall status
`Degraded` because the component is optional.

## Invariants

- **Results are ordered by score, and the handlers restore that order** after loading the media from the database, which returns them in any order.
- **The index is not refreshed** when `media_embeddings` changes; restart the application after filling the table.
- **A reload replaces three fields one by one.** A request running during a reload could see ids and vectors from different loads. Reloads happen only on startup and when the index is empty; see [issues.md](../issues.md).
- `take` must be positive; there is no upper limit, the number of vectors bounds the result.
