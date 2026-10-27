# Dates and time zones of the trip

Media and days are organized by the **local time of the trip**, not by UTC. A photo taken in Kyoto at 9 in the morning belongs to the morning of that day in Kyoto, wherever the server or the visitor is. The time of every media file is stored as that local time, without a time zone, and the day of a media file is simply the date of this time.

## Where the time comes from

Phones and cameras name files by the local time of shooting: `20260301_120000.jpg`, `IMG_20260301_120000.jpg`, `PXL_20260301_120000123.jpg`, `2026-03-01 12.00.00.jpg` and similar. When the name contains such a timestamp, it is taken as the local time as is.

When the name has no timestamp, the modification time of the file is used. It is a moment in UTC, so it has to be converted to the local time of the trip. A trip can cross time zones, so the conversion uses a list of ranges in the configuration (`MediaFormat:TravelDateOffsets`): each range has a UTC start and end and the offset of the local time in hours. The first range containing the moment gives the offset. For example, three months in Japan (+9) and two days in China (+8) are two ranges.

A moment that falls into no range is outside the trip: sync skips such a file, and a YouTube video published at such a moment cannot be imported.

## Things to keep in mind

- **The modification time must survive the upload.** Some tools that copy files to a server reset it to the time of the upload. A file without a timestamp in its name then gets the upload date - or no date at all, if the upload happened after the trip.
- **Files named by a camera in another time zone** are taken at face value: the name is trusted to be the local time of the trip.
- **YouTube videos** use their publish time, not the time of filming, because YouTube does not tell the latter. To add a video published after the trip, the last range has to be extended.
