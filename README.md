# PlaylistToSpotify
Create a spotify playlist from a M3U


This small quick and dirty app will let you take a M3U playlist and upload it to spotify.
There are a lot of scripts online that do this, but every single one I found uses the first song it finds, so if you have "Over the rainbow,"
it'll just pick the first artist it finds, which makes it wrong 99% of the time.

Usage:
Set debug to false, you'll want to see which songs it can't find before making it push to spotify, you may just need to change the title slightly (e.g & vs and)

Again, this is just quick and dirty, so I highly recommend splitting your music into separate m3u playlists based on artist. Since it may not
find everything correctly (for example, it sometimes picks up a live version of a song)

My music organized as such : D:\Users\Paul\Music\Paul Music\{artist}\{album}\{song}.mp3
You will need to update the code to how your music is organized, or comment out that part, since it rarely find it based on the file name.

If it can't find in that way, it searches based on the tags of the music file, so you may need to update your tags if your music doesn't have them.

It'll output a list at the end of any songs that it couldn't find in spotify.

Setup: 
Create a client id (https://developer.spotify.com/)
Add it to this line: _clientId = @"ADD YOUR ID HERE";

Start app, select the playlist file, and hit upload.
