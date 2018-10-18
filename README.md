# Shiny ID3 Tagger
### Automatically queries for missing ID3 tags from Web Services like iTunes, Amazon, Musicbrainz or Spotify and aggregates them to the most likely hit

![Main window](https://cloud.githubusercontent.com/assets/21058782/20148484/28893abe-a6ad-11e6-9941-ab1dfded8c24.png)

## Currently supported webservices to query ID3 data
- **7digital**
- **Amazon**
- **Decibel**
- **Deezer**
- **Discogs**
- **Genius**
- **Gracenote**
- **iTunes**
- **Last.fm**
- **Musicbrainz**
- **Musixmatch**
- **Napster**
- **Netease**
- **Qobuz**
- **QQ**
- **Spotify**
- **Tidal**

## Currently supported webservices to query lyrics
- **Apiseeds**
- **Chartlyrics**
- **Lololyrics**
- **Netease**
- **Viewlyrics**
- **Xiami**

## Key Features
- Queries 17 databases and selects/adds the most frequent results for
  - **Artist name**
  - **Track title**
  - **Album name**
  - **Release date**
  - **Genre**
  - **Disc count**
  - **Disc number**
  - **Track count**
  - **Track number**
- Finds and embeds an **album cover**
- Finds and embeds lyrics
- All tags are written as ID3v2.3
- Fixes encoding errors in tags. All tags will be resaved as UTF16 strings

## How To Use
1. [Download and extract latest release](https://github.com/ShinyId3Tagger/Shiny-ID3-Tagger/releases/latest)
2. Run "Shiny ID3 Tagger.exe" 
3. Program is portable and stealth (nothing get's written to registry or AppData)

## Future Features
- [ ] Implement update check for application (Github shall be the source for downloading newest app files)
- [ ] Add UI for all options of the application
- [ ] Add User option to choose from tags service manually for each file
- [ ] Add User option to en/disable single APIs (sometimes they are down temporarily)
- [ ] Add User option to choose ID3v2.3 (UTF16, Windows 7) or ID3v2.4 (UTF8, Mac)
- [ ] Add User option to write ID3v1 tags additionally to ID3v2
- [ ] Add User option to choose if unknown tags should be removed

## Contributors
<!-- ALL-CONTRIBUTORS-LIST:START - Do not remove or modify this section -->
<!-- prettier-ignore -->
 | <img src="https://avatars1.githubusercontent.com/u/21058782?v=2" width="100"><br /><b>[ShinyId3Tagger](https://github.com/ShinyId3Tagger)</b> | <img src="https://avatars3.githubusercontent.com/u/16746759?v=3" width="100px;"/><br/><b>[Raz Luvaton](https://github.com/rluvaton)</b>
 |  ----- | ----- |
<!-- ALL-CONTRIBUTORS-LIST:END -->
