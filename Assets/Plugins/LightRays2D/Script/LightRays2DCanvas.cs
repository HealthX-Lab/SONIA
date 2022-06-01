using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode, RequireComponent(typeof(Image))]
public class LightRays2DCanvas:LightRays2DAbstract{
	
	private Image img;

	protected override void GetReferences(){
		img=GetComponent<Image>();
	}

	protected override Material GetMaterial(){
		return Instantiate(img.material);
	}

	protected override void ApplyMaterial(Material material){
		img.material=material;
	}

	protected override void Update(){
		base.Update();
	}
}
