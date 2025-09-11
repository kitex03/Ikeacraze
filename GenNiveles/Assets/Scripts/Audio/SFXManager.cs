using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public AudioSource sfxSource;
    //public AudioClip soundFootstep;
    public AudioClip chestOpenClip;
    public AudioClip dropItemClip;
    public AudioClip trashItemClip;
    public AudioClip soundCrafting;     // Suono durante il crafting
    public AudioClip craftingComplete;
    public AudioClip soundDeliver;      // Suono per la consegna
    

/*
    public void soundFootstep()
    {
        sfxSource.PlayOneShot(footstepClip);
    }
*/
    public void PlayChestOpen()
    {
        sfxSource.PlayOneShot(chestOpenClip);
    }

    public void PlayDropItem()
    {
        sfxSource.PlayOneShot(dropItemClip);
    }

    public void PlayTrashItem()
    {
        sfxSource.PlayOneShot(trashItemClip);
    }


    public void PlaySoundCrafting()
    {
        sfxSource.PlayOneShot(soundCrafting);
    }



    public void PlayCraftingComplete()
    {
        sfxSource.PlayOneShot(craftingComplete);
    }



    public void PlaySoundDeliver()
    {
        sfxSource.PlayOneShot(soundDeliver);
    }


}
