using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class ColorUtils : MonoBehaviour {


	/**
	 * 
	 * @see http://baba-s.hatenablog.com/entry/2014/11/28/235900
	 * */
	/// <summary>
	/// 指定された 16 進数を色に変換します
	/// </summary>
	/// <example>
	/// <code>
	/// // RGBA(1.000, 0.502, 0.000, 1.000)
	/// ColorUtils.ToRGB( 0xFF8000 ) 
	/// </code>
	/// </example>
	public static Color  ToRGB(uint val){

		float inv = 1f / 255f;
		Color c = Color.black;

		c.r = inv * ((val >> 16) & 0xFF);
		c.g = inv * ((val >> 8 ) & 0xFF);
		c.b = inv * (val & 0xFF);
		c.a = 1f;
		return c;
	}

	public static Color  ToRGBA(uint val){
		
		float inv = 1f / 255f;
		Color c = Color.black;
		
		c.r = inv * ((val >> 32) & 0xFF);
		c.g = inv * ((val >> 16) & 0xFF);
		c.b = inv * ((val >> 8 ) & 0xFF);
		c.a = inv * (val & 0xFF);

		return c;
	}
}
