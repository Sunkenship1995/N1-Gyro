using UnityEngine;
using System.Collections;

public class moveWallParts : MonoBehaviour {

	private IEnumerator coroutine;

	private IEnumerator ChangeColorOfGameObject(GameObject targetObject, Color color,bool one = true){
		
		//入力されたオブジェクトのRendererを全て取得し、さらにそのRendererに設定されている全Materialの色を変える
		foreach(Renderer targetRenderer in targetObject.GetComponents<Renderer>()){
			foreach(Material material in targetRenderer.materials){
				material.color = color;
				if(one){
					yield return new WaitForSeconds (0.2f);
					// Prints 5.0
					material.color = Color.white;
					
				}
			}
		}
		
		//入力されたオブジェクトの子にも同様の処理を行う
		for(int i = 0; i < targetObject.transform.childCount; i++){
			coroutine = ChangeColorOfGameObject (targetObject.transform.GetChild(i).gameObject, color);
			StartCoroutine(coroutine);
		}
		
	}
	
	
	void OnCollisionEnter2D (Collision2D c){
		
		
		if(c.gameObject.tag == "Player"){
			Debug.Log ("WallHit");
			
			coroutine = ChangeColorOfGameObject (this.gameObject, Color.red,true);
			StartCoroutine(coroutine);
		}
		
		
		//Destroy(gameObject);
	}
}
