using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Additional Enums

public enum MathEquation
{
	MoreThan,
	LessThan,
	MoreThanEqualTo,
	LessThanEqualTo,
	EqualTo,
}

#endregion

public static class Extensions
{
	#region Get Random Array
	public static T GetRandom<T>(this T[] array)
	{
		Debug.Assert(array != null && array.Length != 0);

		return array[Random.Range(0, array.Length)];
	}

	public static T GetRandom<T>(this List<T> list)
	{
		Debug.Assert(list != null && list.Count != 0);

		return list[Random.Range(0, list.Count)];
	}
	#endregion

	public static void DestroyAllChildren(this Transform t)
	{
		for (int i = t.childCount - 1; i >= 0; i--)
		{
			Object.Destroy(t.GetChild(i).gameObject);
		}
	}

	#region Vector manipulation
	public static Vector3 XOZ(this Vector3 vector)
	{
		return new Vector3(vector.x, 0, vector.z);
	}
	public static Vector3 OYZ(this Vector3 vector)
	{
		return new Vector3(0, vector.y, vector.z);
	}
	public static Vector3 XYO(this Vector3 vector)
	{
		return new Vector3(vector.x, vector.y, 0);
	}
	#endregion

	public static bool IsEven(this byte value)
	{
		return value % 2 == 0;
	}

	public static void SafeStopCoroutine(this Coroutine routine, MonoBehaviour m)
	{
		if (routine != null)
		{
			m.StopCoroutine(routine);
		}
	}

	public static string FormatNumber(this int num)
	{
		if (num <= 0) return num.ToString();

		switch (num % 100)
		{
			case 11:
			case 12:
			case 13:
				return num + "th";
		}

		switch (num % 10)
		{
			case 1:
				return num + "st";
			case 2:
				return num + "nd";
			case 3:
				return num + "rd";
			default:
				return num + "th";
		}
	}
	public static bool IsNullOrEmpty(this string str)
	{
		return string.IsNullOrEmpty(str);
	}

	#region Maths
	public static bool MeetsEquation(this float f, MathEquation equation, float compareValue)
	{
		switch (equation)
		{
			case MathEquation.EqualTo:
				return f == compareValue;
			case MathEquation.LessThan:
				return f < compareValue;
			case MathEquation.LessThanEqualTo:
				return f <= compareValue;
			case MathEquation.MoreThan:
				return f > compareValue;
			case MathEquation.MoreThanEqualTo:
				return f >= compareValue;
		}

		return false;
	}

	public static bool MeetsEquation(this int f, MathEquation equation, int compareValue)
	{
		switch (equation)
		{
			case MathEquation.EqualTo:
				return f == compareValue;
			case MathEquation.LessThan:
				return f < compareValue;
			case MathEquation.LessThanEqualTo:
				return f <= compareValue;
			case MathEquation.MoreThan:
				return f > compareValue;
			case MathEquation.MoreThanEqualTo:
				return f >= compareValue;
		}

		return false;
	}

	public static float GetAverage(this float[] floatArray)
	{
		Debug.Assert(floatArray.Length > 0);
		
		float total = 0;
		for (int i = 0; i < floatArray.Length; i++)
		{
			total += floatArray[i];
		}

		return total / floatArray.Length;
	}
	public static float GetAverage(this List<float> floatList)
	{
		Debug.Assert(floatList.Count > 0);

		float total = 0;
		for (int i = 0; i < floatList.Count; i++)
		{
			total += floatList[i];
		}

		return total / floatList.Count;
	}
	#endregion
}
