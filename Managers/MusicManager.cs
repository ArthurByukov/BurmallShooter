using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // Источник для музыки

    [Header("Music Track")]
    [SerializeField] private AudioClip combatMusic; // Музыка для волн

    [Header("Settings")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 1.5f;

    private float originalVolume;
    private Coroutine currentFade;

    private void Awake()
    {
        // Создаём источник, если не назначен
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true; // Автоматическое зацикливание
            musicSource.playOnAwake = false;
        }

        originalVolume = musicSource.volume;
        musicSource.clip = combatMusic;
    }

    // Вызывать при старте волны
    public void StartWaveMusic()
    {
        StopCurrentFade();

        // Если музыка уже играет и это правильный трек — не трогаем
        if (musicSource.isPlaying && musicSource.clip == combatMusic)
            return;

        musicSource.clip = combatMusic;
        musicSource.loop = true;
        musicSource.volume = 0f;
        musicSource.Play();

        currentFade = StartCoroutine(FadeVolume(0f, originalVolume, fadeInDuration));
    }

    // Вызывать при окончании волны
    public void StopWaveMusic()
    {
        if (!musicSource.isPlaying) return;

        StopCurrentFade();
        currentFade = StartCoroutine(FadeVolume(originalVolume, 0f, fadeOutDuration, true));
    }

    // Полная остановка без фейда (если нужно)
    public void StopImmediately()
    {
        StopCurrentFade();
        musicSource.Stop();
        musicSource.volume = originalVolume;
    }

    private void StopCurrentFade()
    {
        if (currentFade != null)
            StopCoroutine(currentFade);
    }

    private IEnumerator FadeVolume(float from, float to, float duration, bool stopAfter = false)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            musicSource.volume = Mathf.Lerp(from, to, t);
            yield return null;
        }

        musicSource.volume = to;

        if (stopAfter)
        {
            musicSource.Stop();
            musicSource.volume = originalVolume; // Сброс для будущего воспроизведения
        }
    }
}
