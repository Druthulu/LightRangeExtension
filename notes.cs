light range

ControllerGUI.cs
	public float lightIntensity = 2f;
	// orig 0.75f


	// Prefix Update(), set value, return true, let original run
	//no
	
	// postfix on start, edit variable
	Start() //maybe jack into and set the value
		// if this is a contructor
		[HarmonyPatch(typeof(DialogResponseQuest))]
		public static class DialogResponseQuestPatch
		{
			[HarmonyPatch(MethodType.Constructor)]
				[HarmonyPatch(new Type[]
				{
					typeof(string),
					typeof(string),
					typeof(string),
					typeof(string),
					typeof(Dialog),
					typeof(int),
					typeof(int)
				})]
				[HarmonyPostfix]
				public static void DialogResponseQuestPostfix(DialogResponseQuest __instance, string _questID, string _nextStatementID, string _returnStatementID, string _type, Dialog _ownerDialog, int _listIndex = -1, int _tier = -1)
			}



LightLOD.cs
	// prefix, return false, use new value
	public void CalcViewDistance()
	{
		this.lightViewDistance = Utils.FastMax(this.MaxDistance, this.lightRangeMaster * 25f);
	}
	// orig 1.5f

	// prefix, return false, use new value
	public void FrameUpdate(Vector3 cameraPos)
	{
		this.priority = 0f;
		if (this.bRenderingOff || this.lightStateEnabled)
		{
			return;
		}
		this.CheckInitialBlock();
		if (!this.bSwitchedOn)
		{
			return;
		}
		Light light = this.myLight;
		if (light)
		{
			float num = (this.selfT.position - cameraPos).sqrMagnitude * this.DistanceScale;
			float num2 = Mathf.Sqrt(num) - this.lightRange;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			float num3 = this.lightViewDistance;
			if (this.bPlayerPlacedLight)
			{
				num3 *= 25f; // ORIG 1.2f
			}
			if (LightLOD.DebugViewDistance > 0f)
			{
				num3 = Utils.FastMax(LightLOD.DebugViewDistance, this.lightRange + 0.01f);
			}
			float num4 = num3 * num3;
			this.distSqRatio = num / num4;
			if (this.bToggleable)
			{
				this.LightStateCheck();
			}
			float num5 = num3 - this.lightRange;
			if (num2 < num5)
			{
				this.priority = 1f;
				if (this.bPlayerPlacedLight)
				{
					if (this.distSqRatio >= 0.640000045f)
					{
						light.shadows = LightShadows.None;
					}
					else if (this.distSqRatio >= 0.0625f)
					{
						if (this.shadowStateMaster == LightShadows.Soft)
						{
							light.shadows = LightShadows.Hard;
						}
						light.shadowStrength = (1f - Utils.FastClamp01((this.distSqRatio - 0.36f) / 0.280000031f)) * this.shadowStrengthMaster;
					}
					else
					{
						light.shadows = this.shadowStateMaster;
						light.shadowStrength = this.shadowStrengthMaster;
					}
				}
				float num6 = num2 / num5;
				float num7 = 1f - num6 * num6;
				light.intensity = this.lightIntensity * num7;
				light.range = this.lightRange * 0.75f + this.lightRange * 0.75f * num7; //ORIG 0.5 on both
				light.enabled = true;
			}
			else
			{
				light.enabled = false;
			}
			if (this.lensFlare != null)
			{
				if (num < 10f * num4)
				{
					float num8 = (1f - num / (num4 * 10f)) * this.lightIntensity * 0.33f * this.FlareBrightnessFactor;
					if (num8 > 1f)
					{
						num8 = 1f;
					}
					if (this.lightRange < 4f)
					{
						num8 *= this.lightRange * 0.25f;
					}
					this.lensFlare.brightness = num8;
					this.lensFlare.color = light.color;
					this.lensFlare.enabled = true;
					return;
				}
				this.lensFlare.enabled = false;
			}
		}
	}


SkyManager.cs	

	// prefix, return false, use new value
	public static void SetSunIntensity(float i)
	{
		SkyManager.sunIntensity = i;
		float sunAngle = SkyManager.GetSunAngle();
		if (sunAngle >= -SkyManager.sSunFadeHeight)
		{
			SkyManager.sunIntensity = -sunAngle * 10f * SkyManager.sunIntensity * (float)((sunAngle < 0f) ? 1 : 0);
		}
		SkyManager.sunIntensity = Mathf.Clamp(SkyManager.sunIntensity, 0f, SkyManager.sMaxSunIntensity);
		SkyManager.sunIntensity *= 1.1f; // ORIG 1.5f
		if (SkyManager.sunLight != null)
		{
			SkyManager.sunLight.intensity = SkyManager.sunIntensity * SkyManager.fogLightScale;
		}
	}


	// prefix, return false, use new value
	public static float GetMoonAmbientScale(float add, float mpy)
	{
		return Utils.FastLerp(add + SkyManager.moonBright * mpy, 0.7f, SkyManager.dayPercent * 3.030303f); // ORIG 1f
	}




TileEntityLight.cs
	public float LightRange = 150f; // ORIG 10f

	// Consturctor postfix, edit value
	public TileEntityLight(Chunk _chunk) : base(_chunk)
	{
	}