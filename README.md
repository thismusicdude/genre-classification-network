# genre-classification-network

## Software Setup

add a `.env` file in repository and fill it with the following environment variables
```
CLIENT_ID="<client id>"
CLIENT_SECRET="<secret token>"
```
you can get those tokens from the [Spotify Web API Developer Console ](https://developer.spotify.com/documentation/web-api)

## Aim of this project

The aim of this project is to develop a more aesthetically pleasing and user-friendly solution. The focus here is on improved visualization, usability and user-friendliness in order to facilitate the exploration of musical genres from user libraries and make it a pleasant experience.

## How does it work?

1. *authentication and data retrieval*:
   * The user logs in to Spotify (see [Figure 1](#fig:spotify_login)) to grant access to their listening habits. The data is then retrieved via the Spotify API.  
2. *data processing*:
   * The genres of the most listened to artists are extracted and transferred into a meaningful hierarchy.  
3. *Graph generation*:
   * Based on the processed data, the **FDG** is generated. The main genres are positioned at regular intervals around the root node, while subgenres are distributed dynamically.  
4. *interaction*:
    * The user can manually move or zoom in on the graph to get a customized view.  

For a visually appealing design of the application, a shader reminiscent of plasma was implemented for certain backgrounds and textures. This was based on an already published shader from a [godot user](youtube.com/watch?v=0Rcxr76-3Ms) and was modified to server our puproses.
![image](https://github.com/user-attachments/assets/87e26d39-e880-40d3-b2f5-93c2c176033e)
