# FMOD-Unity: Rhythm Game Tools

this repo includes various scripts for a rhythm game i'm working on, hopefully someone can find use of them

## Unity Scripts

### MusicManager

The music manager script takes an FMOD event reference, plays it and records the data of the event as it plays. The data recorded is as follows:
- Total length of the song in milliseconds
- Current playback position in milliseconds
- Current beat/bar
- Current tempo
- Name of last marker passed

The length of the song is gathered from the FMOD event description in the awake function of the script.  
The playback position is updated every frame in the update function of the script.  
Beat, bar, and tempo are updated in a callback function that is executed when beat events are passed.  
Last marker name is updated in a callback function that is executed when timeline marker events are passed.  


In most use cases seperate functions will want to be executed in the callback function rather than just recording the data, and a comment block is left to signal where you would want to put these calls.

### Decoder

Decodes the marker names into a dictionary of arguments which can then be used similar to kwargs from Python. To note all the data is stored as strings so you will have to parse them into whatever data type you need when accessing them.

## FMOD Scripts

### Create/Edit Marker

### Shift Marker
