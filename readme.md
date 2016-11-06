# Shiny ID3 Tagger
### Automatically searches and adds missing ID3 tags for your music collection with just two clicks

![Main window](https://cloud.githubusercontent.com/assets/21058782/20035217/6e4d4f2e-a3db-11e6-9e9d-3344ee8ce90b.png)


## Features
- The app extracts ID3 tags for artist and title or tries to guess them from filename (filename pattern: artist - title)
- Queries 16 different online web services to retrieve as much additional data as possible
- All retrieved values are weighted and the best match is taken for:
  - artist name
  - track title
  - album name
  - release date
  - genre
  - disc count
  - disc number
  - track count
  - track number
- Artist, title, Album and genre have the correct casing and spelling mistakes are corrected
- Adds embedded cover which matches the best album name
- Adds lyrics as ID3 tag
- If no album was found (mashups, remixes) the first image from bing image search is used as fallback cover (optional)

## Currently supported webservices
- 7digital
- Amazon
- Quantonemusic (formerly Decibel)
- Deezer
- Discogs
- Genius
- Gracenote
- iTunes
- Last.fm
- Microsoft Groove
- Musicbrainz
- Musicgraph
- Musixmatch
- Napster
- Qobuz
- Spotify
- Lololyrics (only for lyrics)
- Bing (only as fallback image source)

## How To Install
- [Download and extract the latest release](https://github.com/ShinyId3Tagger/Shiny-ID3-Tagger/releases/latest)
- Run "Shiny Id3 Tagger.exe"

