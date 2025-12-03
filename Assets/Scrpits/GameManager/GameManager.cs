using UnityEngine;

public class GameManager : SingletonMonobehaviour<GameManager>
{
    public Weather currentWeather;

    protected void Awake()
    {
        base.Awake();

        //TODO: Need a resolution settings options screen
        Screen.SetResolution(1920, 1080, true);

        // Set starting weather 
        currentWeather = Weather.dry;
    }

}