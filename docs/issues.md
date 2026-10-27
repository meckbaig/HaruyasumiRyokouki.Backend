# Known issues

Problems found during the refactoring and the documentation pass that were left as they are,
on purpose or for later. An entry is deleted once it is fixed or decided against.

## How this list is ranked

| Severity | Meaning |
| --- | --- |
| `visible` | Someone can see something wrong today. |
| `latent` | Correct today, breaks on a plausible change or a rare timing. |
| `maintenance` | Nothing breaks, but the code costs more to change. |

| Effort | Meaning |
| --- | --- |
| `small` | One file, a few lines. |
| `medium` | Several files, needs a test run against a database. |

## Issues


### I2. One failed save breaks the rest of a sync - `latent`, `small`

`MediaSyncService` shares one `DbContext` for the whole sync. When saving one media file fails, its
entities stay in the change tracker and every later `SaveChanges` of the same sync fails on them
too. Seen only with parallel syncs, which the sync lock now prevents. Fix: detach the failed
entities (or clear the tracker) in the `catch` of `CheckForNewFiles`. Decided to leave for now.

### I3. Similarity index reload is not atomic - `latent`, `small`, very low priority

`MediaSimilarityIndexService.ReloadAsync` assigns ids, vectors and the row map one after another.
A request running at that moment may combine data of two loads: wrong scores or an index out of
range. Reloads happen only on startup and when the index is empty. Fix: keep the three in one
object and swap the reference. Postponed together with the review of `media_embeddings`.

### I4. Queued file removals are lost on restart - `latent`, `medium`

`FileRemovalBackgroundService` keeps the queue in memory. Files queued but not yet deleted when the
application stops stay in the storage forever, and nothing reports them: they are no longer in the
database, so sync does not list them as missing. Fix needs a persistent queue (a table) or a
"files in storage but not in database" report in sync.

### I5. The date in the body of `PUT /v1/days/{date}` is ignored - `maintenance`, `small`

`DayEditDto.Date` is part of the contract but `FromEditDto` does not apply it; the day is always the
one from the route. Decided not to change: days are not moved. The field could be documented in
Swagger as ignored or removed from the edit DTO.

