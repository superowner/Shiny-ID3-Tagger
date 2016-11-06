# Shiny ID3 Tagger
A straight-forward windows application to automatically retrieve missing ID3 tags for your music files. 

1. The app extracts ID3 tags for artist and title or tries to guess them from filename (filename pattern: artist - title)
2. Queries 16 different online web services to retrieve as much additional data as possible
3. All retrieved values are weighted and the best match is taken
  - correct artist name
  - correct track title
  - album name
  - release date
  - genre
  - disc count
  - disc number
  - track count
  - track number
4. An embedded cover is added which matches the best album name
5. If no album was found (ie mashup, remixes) the first image from a a simple bing image search is used as fallback (optional)
5. Lyrics are added as ID3 tag if found on one of the implemented lyrics websites

![Main window](https://cloud.githubusercontent.com/assets/21058782/20035217/6e4d4f2e-a3db-11e6-9e9d-3344ee8ce90b.png)


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

# How To Install
- todo

# How To Use
- todo

# How to add a new RESTful Web Service
- todo
