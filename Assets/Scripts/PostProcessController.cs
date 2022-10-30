using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessController : MonoBehaviour {

    [Header("Internal References")]
    [SerializeField] private PostProcessVolume volume;
    [SerializeField] private PostProcessLayer layer;
    [SerializeField] private PostProcessProfile profile;

    [Header("External References")]
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Preferences")]
    [SerializeField] private float minimumAberrationSpeed;
    [SerializeField] private float aberrationCoefficient;

    private ChromaticAberration aberrationEffectSettings;
    private Bloom bloomEffectSettings;
    private ColorGrading colorEffectSettings;

    /// <summary>
    /// Calculates how much chromatic abberation to apply given a player's speed
    /// </summary>
    /// <param name="playerSpeed">The current speed of the player</param>
    /// <returns></returns>
    private float CalculateAberrationIntensity ( float playerSpeed ) {
        if ( playerSpeed < minimumAberrationSpeed ) return 0;

        return Mathf.Log(aberrationCoefficient * (playerSpeed - minimumAberrationSpeed));
    }

    /// <summary>
    /// Called the first frame the script is active
    /// </summary>
    private void Start () {
        aberrationEffectSettings = profile.GetSetting<ChromaticAberration>();
        //bloomEffectSettings = profile.GetSetting<Bloom>();
        //colorEffectSettings = profile.GetSetting<ColorGrading>();
    }

    private void Update () {
        float playerSpeed = playerRigidbody.velocity.magnitude;

        float intensity = CalculateAberrationIntensity(playerSpeed);

        aberrationEffectSettings.intensity.value = intensity;
    }
}
