﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using rnd = ThunderWire.Helper.Random;

public class HealthManager : MonoBehaviour {

	public ScriptManager scriptManager;
	private HFPS_GameManager gameManager;
    private CameraBloodEffect bloodEffect;
    private PlayerController player;
    private rnd.Random rand = new rnd.Random();

    [Header("Health Settings")]
	public float Health = 100.0f;
    public float maximumHealth = 200.0f;
    public float lowHealth = 15f;
    public float maxRegenerateHealth = 100.0f;

    [Header("Player Low Health")]
    public bool lowHealthSettings;
    public float lowWalkSpeed;
    public float lowRunSpeed;

    private float normalWalk;
    private float normalRun;

    [Header("Regeneration")]
    public bool regeneration = false;
    public float regenerationSpeed;
    public float timeWaitAfterDamage;

    [Header("Pain")]
    public float painStopTime;
    public float maxPainAmount;

    [Header("Audio Settings")]
	public AudioClip[] DamageSounds;
    [Range(0, 1)]
    public float Volume = 1f;

    [Header("Colors")]
    public Color HealthColor = new Color(255, 255, 255);
    public Color LowHealthColor = new Color(0.9f, 0, 0);

    private Text HealthText;

    private Color CurColor = new Color(0, 0, 0);

    private bool lowHealthMode;

    [HideInInspector]
	public bool isMaximum;

    [HideInInspector]
    public bool isDead;

    private void Awake()
    {
        bloodEffect = Camera.main.GetComponent<CameraBloodEffect>();
        player = GetComponent<PlayerController>();
    }

    void Start()
	{
        gameManager = scriptManager.GetScript<HFPS_GameManager>();
		HealthText = gameManager.HealthText;
		CurColor = HealthColor;
        bloodEffect.bloodAmount = 0;
        normalWalk = player.walkSpeed;
        normalRun = player.runSpeed;
	}
	
	void Update()
	{
        if (isDead) return;

		if(HealthText)
		{
			HealthText.text = System.Convert.ToInt32(Health).ToString();
			HealthText.color = CurColor;
		}

        if (Health <= lowHealth)
        {
            CurColor = Color.Lerp(CurColor, LowHealthColor, (Seno(6.0f, 0.1f, 0.0f) * 5) + 0.5f);
            if (!lowHealthMode && bloodEffect.bloodAmount != maxPainAmount)
            {
                StopAllCoroutines();
                bloodEffect.bloodAmount = maxPainAmount;

                if (lowHealthSettings)
                {
                    player.walkSpeed = lowWalkSpeed;
                    player.runSpeed = lowRunSpeed;
                }

                lowHealthMode = true;
            }
        }
        else if (lowHealthMode)
        {
            StopAllCoroutines();
            StartCoroutine(PainFade(2));
            CurColor = Color.Lerp(CurColor, HealthColor, (Seno(6.0f, 0.1f, 0.0f) * 5) + 0.5f);

            if (lowHealthSettings)
            {
                player.walkSpeed = normalWalk;
                player.runSpeed = normalRun;
            }

            lowHealthMode = false;
        }
		
		if(Health <= 0 || Health <= 0.9)
		{
			Health = 0f;
            gameManager.ShowDeadPanel();

            isDead = true;
        }

		if (Health >= maximumHealth) {
			Health = maximumHealth;
			isMaximum = true;
		} else {
			isMaximum = false;
		}
    }

    public void ApplyDamage(float damage)
    {
        if (Health <= 0) return;
        Health -= damage;

        if (DamageSounds.Length > 0)
        {
            GetComponent<AudioSource>().PlayOneShot(DamageSounds[rand.Range(0, DamageSounds.Length)], Volume);
        }

        if (regeneration)
        {
            StopCoroutine(Regenerate()); //Stop current running regeneration
            StartCoroutine(Regenerate()); //Start new regeneration
        }

        if (Health <= lowHealth)
        {
            StopAllCoroutines();
            bloodEffect.bloodAmount = maxPainAmount;

            if (lowHealthSettings)
            {
                player.walkSpeed = lowWalkSpeed;
                player.runSpeed = lowRunSpeed;
            }

            lowHealthMode = true;
        }
        else
        {
            float pain = bloodEffect.bloodAmount + 0.1f;

            if (pain >= maxPainAmount)
            {
                pain = maxPainAmount;
                bloodEffect.bloodAmount = pain;
            }
            else
            {
                bloodEffect.bloodAmount = pain;
            }

            StopAllCoroutines();
            StartCoroutine(PainFade(2));
        }
    }
	
	public void ApplyHeal(float heal)
	{
		if (Health > 0 && !isMaximum)
        {
            Health += heal;
        }
		if (isMaximum) {
            gameManager.WarningMessage ("You have maximum health");
		}
    }
	
    public static float Seno(float rate, float amp, float offset = 0.0f)
    {
        return (Mathf.Cos((Time.time + offset) * rate) * amp);
    }

    IEnumerator PainFade(float wait)
    {
        yield return new WaitForSeconds(wait);

        var currentValue = bloodEffect.bloodAmount;
        var t = 0f;

        while (t < 1)
        {
            t += Time.deltaTime / (painStopTime * 10);
            bloodEffect.bloodAmount = Mathf.Lerp(currentValue, 0f, t);
            yield return null;
        }
    }

    IEnumerator Regenerate()
    {
        yield return new WaitForSeconds(timeWaitAfterDamage);

        while (Health <= maxRegenerateHealth)
        {
            Health += Time.deltaTime * regenerationSpeed;
            yield return null;
        }
    }
}
