# Similar media

Tagging one photo is rarely tagging one photo: the same subject was usually shot several times in a row, and again from another side a week later. Similarity search helps the editor find such photos. It has two uses:

- **similar to this media** - photos that look like the one being edited;
- **what else belongs to this tag** - photos that look like the ones that already have the tag.

## How it works

A photo or video can have an *embedding*: a vector of 768 numbers produced by an image model (SigLIP 2). Photos that look alike have vectors that point in similar directions, so the similarity of two photos is the cosine between their vectors - 1 for the same picture, lower for less similar ones.

The backend does not compute embeddings. They are calculated by a separate tool, [HaruyasumiRyokouki.DataTool](https://github.com/meckbaig/HaruyasumiRyokouki.DataTool), and written straight into the `media_embeddings` table - the only table the tool writes. It always uses the same model (`google/siglip2-base-patch16-256`) and computes vectors exactly like the ones already stored, so old and new vectors can be compared with each other. A video gets the vector of its frame at the first second.

Media added to the site without the tool have no embedding and do not take part in the search until the tool is run for them: its whole cycle when a new batch of files is imported with it, or only its `push-embeddings` step when the media are already on the site. Vectors that are already stored are kept unless the tool is explicitly asked to replace them, and at the end it lists the media that still have no vector.

On startup the server loads all vectors into memory as one array. A search compares the query vector with every vector in turn - for a few thousand photos it takes milliseconds, and no special index is needed.

- For "similar to this media" the query is the vector of that media.
- For "what else belongs to this tag" the query is the average direction of all media that already have the tag. Media that have it are excluded from the result. With fewer than three tagged media the server does not guess: one or two photos do not describe a subject, so the result is empty.

There is no similarity threshold. How alike is alike enough depends on the subject - "this exact gate" and "a shrine" need different lines - so the server returns a list sorted by score, and the editor decides where it stops being useful.

## Turning it off

The vectors take memory, about 3 KB per photo. `Application:SimilaritySearchEnabled = false` skips loading them, and both similarity endpoints answer 503.

The index is not reloaded when the table changes: after the tool writes new embeddings, the application has to be restarted.
