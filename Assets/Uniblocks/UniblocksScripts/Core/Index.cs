using UnityEngine;
using System.Collections;

// a Vector3 using ints instead of floats, for storing indexes and stuff

namespace Uniblocks {

public class Index {

	public int x,y,z;
	
	public Index ( int setX, int setY, int setZ ) {
		this.x = setX;
		this.y = setY;
		this.z = setZ;
	}
	
	public Index ( Vector3 setIndex ) {
		this.x = (int)setIndex.x;
		this.y = (int)setIndex.y;
		this.z = (int)setIndex.z;
	}
	
	public Vector3 ToVector3 () {
		return new Vector3 (x,y,z);
	}
	
	public override string ToString () {
		return ( x.ToString() + "," + y.ToString() + "," + z.ToString() );

	}	
	
	public bool IsEqual ( Index to ) {
		
		if (to == null) {
			return false;
		}
		
		if (this.x == to.x &&
			this.y == to.y &&
			this.z == to.z) {
				return true;
			}
		else return false;	
	}
	
	public Index GetAdjacentIndex ( Direction direction ) {
		if (direction == Direction.down) 			return new Index(x,y-1,z);
		else if (direction == Direction.up)	 		return new Index(x,y+1,z);
		else if (direction == Direction.left) 		return new Index(x-1,y,z);
		else if (direction == Direction.right) 		return new Index(x+1,y,z);
		else if (direction == Direction.back) 		return new Index(x,y,z-1);
		else if (direction == Direction.forward) 	return new Index(x,y,z+1);
		else return null;
	}
	
	public static bool Compare ( Index a, Index b ) {
	
		if (b == null) {
			return false;
		}
	
		if (a.x == b.x &&
			a.y == b.y &&
			a.z == b.z) {
				return true;
			}
		else return false;
	}
	
	public static Index FromString ( string indexString ) {
	
		string[] splitString = indexString.Split (',');
		
		try {
			return new Index (int.Parse (splitString[0]), int.Parse (splitString[1]), int.Parse (splitString[2]));
		}
		catch (System.Exception) {
			Debug.LogError ("Uniblocks: Index.FromString: Invalid format. String must be in \"x,y,z\" format.");
			return null;
		}
		
	}

}

}
