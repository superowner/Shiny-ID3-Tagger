# Shiny ID3 Tagger
### Automatically queries for missing ID3 tags from Web Services like iTunes, Amazon, Musicbrainz or Spotify and aggregates them to the most likely hit

![Main window](https://cloud.githubusercontent.com/assets/21058782/20148484/28893abe-a6ad-11e6-9941-ab1dfded8c24.png)

## Currently supported webservices to query data
- **7digital**
- **Amazon**
- **Decibel**
- **Deezer**
- **Discogs**
- **Genius**
- **Gracenote**
- **iTunes**
- **Last.fm**
- **Microsoft Groove**
- **Musicbrainz**
- **Musicgraph**
- **Musixmatch**
- **Napster**
- **Netease**
- **Qobuz**
- **QQ**
- **Spotify**
- **Tidal**
- **Viewlyrics** (only for lyrics)
- **Chartlyrics** (only for lyrics)
- **Lololyrics** (only for lyrics)
- **Viewlyrics** (only for lyrics)
- **Xiami** (only for lyrics)

## Key Features
- Queries 19 online databases at the same time and selects/adds the most common results.
  This means, if for example 10 webservices respond with "Beyoncé" as artist name and only 5 webservices say it's "Beyonce" (note the difference) than the
  program assumes that the majority is right and selects the first variant. This is the way it works for all tags
- Supports the following ID3v2.3 tags
  - **Artist name**
  - **Track title**
  - **Album name**
  - **Release date**
  - **Genre**
  - **Disc count**
  - **Disc number**
  - **Track count**
  - **Track number**
- Auto downloads and embedds the **album cover**
- Adds lyrics as ID3 tag *Unsynced lyrics*

## How To Install
- [Download and extract the latest release](https://github.com/ShinyId3Tagger/Shiny-ID3-Tagger/releases/latest)
- Open the extracted folder and run "Shiny Id3 Tagger.exe". That's it. No further installation needed
- The GUI is pretty simple and self explanatory

