using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource cardDealtSound;
    [SerializeField] AudioSource backgroundMusic;    
    public void  PlayCardDealSound()
    {
        cardDealtSound.Play();
    }
    public void PlayBackgroundMusic()
    {
        backgroundMusic.Play();
    }

    //add other sound effects when needed
}
