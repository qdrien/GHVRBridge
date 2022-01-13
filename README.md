# Grasshopper-Vive-Sharing

Proof-of-concept project allowing users to interact with Grasshopper parameters in Virtual Reality. 
The Grasshopper plugin can stream meshes and share parameters through a WebSockets server, that acts as a relay between that plugin and connected clients (here: a VR application for the HTC Vive).

The project and its attached examples are currently set up for local single-client use but extending it to multiple clients should be easy. Remote access could theoretically be possible with a similar ease but latency could cause issues.

## Installation

1. Put all dlls and gha files inside Libraries folder for Grasshopper (Usually in C:\Users\xxx\AppData\Roaming\Grasshopper\Libraries). Unblock all dlls and gha files.

2. Make sure you have [Microsoft .NET Framework 4.5](https://www.microsoft.com/en-US/download/details.aspx?id=30653) installed.

## Notes

- Be careful not to have duplicate dlls (like Newtonsoft.Json) in the Grasshopper's libraries folder.

## Usage

1. z

2. a

3. a


## Dependencies



## Acknowledgments

https://github.com/jhorikawa/MeshStreamingGrasshopper
