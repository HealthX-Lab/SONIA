using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System;

[CustomEditor(typeof(LightRays2D))]
public class LightRays2DEditor:Editor{

	LightRays2D script;

	[MenuItem("GameObject/Effects/LightRays2D")]
	static void Create(){
		GameObject go=new GameObject();
		go.AddComponent<LightRays2D>();
		go.name="LightRays2D";
		go.transform.position=new Vector3(SceneView.lastActiveSceneView.pivot.x,SceneView.lastActiveSceneView.pivot.y,0f);
		go.transform.localScale=Vector3.one*5f;
		//Set shared mesh to built in quad
		MeshFilter mf=go.GetComponent<MeshFilter>();
		GameObject quadGO=GameObject.CreatePrimitive(PrimitiveType.Quad);
		mf.sharedMesh=quadGO.GetComponent<MeshFilter>().sharedMesh;
		DestroyImmediate(quadGO);
		//Set mesh renderer stuff
		MeshRenderer mr=go.GetComponent<MeshRenderer>();
		mr.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.Off;
		mr.reflectionProbeUsage=UnityEngine.Rendering.ReflectionProbeUsage.Off;
		mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
		mr.receiveShadows=false;
		mr.sharedMaterial=new Material(Shader.Find("Custom/LightRays"));
		//Close inspectors
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(mf,false);
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(mr,false);
		//Select the object
		Selection.activeGameObject=go;
	}

	[MenuItem("GameObject/UI/LightRays2DCanvas")]
	static void CreateCanvas(){
		GameObject go=new GameObject();
		go.AddComponent<LightRays2DCanvas>();
		go.name="LightRays2DCanvas";
		//go.transform.position=new Vector3(SceneView.lastActiveSceneView.pivot.x,SceneView.lastActiveSceneView.pivot.y,0f);
		//go.transform.localScale=Vector3.one*5f;
		go.transform.SetParent(GameObject.Find("Canvas").transform);
		
		Image img=go.GetComponent<Image>();
		img.material=new Material(Shader.Find("Custom/LightRays"));
		//Close inspectors
		UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(img,false);
		//Select the object
		Selection.activeGameObject=go;
	}

	void Awake(){
		script=(LightRays2D)target;
	}

	public override void OnInspectorGUI(){
		DrawDefaultInspector();

		//Sprite sorting
		GUILayout.Space(10);
		//Get sorting layers
		int[] layerIDs=GetSortingLayerUniqueIDs();
		string[] layerNames=GetSortingLayerNames();
		//Get selected sorting layer
		int selected=-1;
		for(int i=0;i<layerIDs.Length;i++){
			if(layerIDs[i]==script.sortingLayer){
				selected=i;
			}
		}
		//Select Default layer if no other is selected
		if(selected==-1){
			for(int i=0;i<layerIDs.Length;i++){
				if(layerIDs[i]==0){
					selected=i;
				}
			}
		}
		//Sorting layer dropdown
		EditorGUI.BeginChangeCheck();
		GUIContent[] dropdown=new GUIContent[layerNames.Length+2];
		for(int i=0;i<layerNames.Length;i++){
			dropdown[i]=new GUIContent(layerNames[i]);
		}
		dropdown[layerNames.Length]=new GUIContent();
		dropdown[layerNames.Length+1]=new GUIContent("Add Sorting Layer...");
		selected=EditorGUILayout.Popup(new GUIContent("Sorting Layer","Name of the Renderer's sorting layer"),selected,dropdown);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(script,"Change sorting layer");
			if(selected==layerNames.Length+1){
				EditorApplication.ExecuteMenuItem("Edit/Project Settings/Tags and Layers");
			}else{
				script.sortingLayer=layerIDs[selected];
			}
			EditorUtility.SetDirty(script);
		}
		//Order in layer field
		EditorGUI.BeginChangeCheck();
		int order=EditorGUILayout.IntField(new GUIContent("Order in Layer","Renderer's order within a sorting layer"),script.orderInLayer);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(script,"Change order in layer");
			script.orderInLayer=order;
			EditorUtility.SetDirty(script);
		}
	}

	//Get the sorting layer IDs
	public int[] GetSortingLayerUniqueIDs() {
		Type internalEditorUtilityType=typeof(InternalEditorUtility);
		PropertyInfo sortingLayerUniqueIDsProperty=internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs",BindingFlags.Static|BindingFlags.NonPublic);
		return (int[])sortingLayerUniqueIDsProperty.GetValue(null,new object[0]);
	}

	//Get the sorting layer names
	public string[] GetSortingLayerNames(){
		Type internalEditorUtilityType=typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty=internalEditorUtilityType.GetProperty("sortingLayerNames",BindingFlags.Static|BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null,new object[0]);
	}
}
