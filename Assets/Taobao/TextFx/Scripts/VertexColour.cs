/**
	TextFx VertexColour class.
	Defines four colours for each vertex in a Quad mesh. Used to apply a specific vertex colour to each corner of a letters mesh.
**/
using UnityEngine;

[System.Serializable]
public class VertexColour
{
	public Color top_left = Color.white;
	public Color top_right = Color.white;
	public Color bottom_right = Color.white;
	public Color bottom_left = Color.white;
	
	public VertexColour(){}
	
	public VertexColour(Color init_color)
	{
		top_left = init_color;
		top_right = init_color;
		bottom_right = init_color;
		bottom_left = init_color;
	}
	
	public VertexColour(Color tl_colour, Color tr_colour, Color br_colour, Color bl_colour)
	{
		top_left = tl_colour;
		top_right = tr_colour;
		bottom_right = br_colour;
		bottom_left = bl_colour;
	}
	
	public VertexColour(VertexColour vert_col)
	{
		top_left = vert_col.top_left;
		top_right = vert_col.top_right;
		bottom_right = vert_col.bottom_right;
		bottom_left = vert_col.bottom_left;
	}

	public Color this[int i]
	{
		get
		{
			switch (i) {
			case 0:
				return top_left;
			case 1:
				return top_right;
			case 2:
				return bottom_right;
			case 3:
				return bottom_left;
			}
			return Color.clear;
		}
		set
		{
			switch (i) {
			case 0:
				top_left = value;
				break;
			case 1:
				top_right = value;
				break;
			case 2:
				bottom_right = value;
				break;
			case 3:
				bottom_left = value;
				break;
			}
		}
	}

	public VertexColour FlatColour { get { return new VertexColour (top_left); } }
	
	public VertexColour Clone()
	{
		VertexColour vertex_col = new VertexColour();
		vertex_col.top_left = top_left;
		vertex_col.top_right = top_right;
		vertex_col.bottom_right = bottom_right;
		vertex_col.bottom_left = bottom_left;
		
		return vertex_col;
	}
	
	public VertexColour Add(VertexColour vert_col)
	{
		VertexColour v_col = new VertexColour();
		v_col.bottom_left = bottom_left + vert_col.bottom_left;
		v_col.bottom_right = bottom_right + vert_col.bottom_right;
		v_col.top_left = top_left + vert_col.top_left;
		v_col.top_right = top_right + vert_col.top_right;
		
		return v_col;
	}

	public void AddInLine(VertexColour vert_col)
	{
		bottom_left += vert_col.bottom_left;
		bottom_right += vert_col.bottom_right;
		top_left += vert_col.top_left;
		top_right += vert_col.top_right;
	}
	
	public VertexColour Sub(VertexColour vert_col)
	{
		VertexColour v_col = new VertexColour();
		v_col.bottom_left = bottom_left - vert_col.bottom_left;
		v_col.bottom_right = bottom_right - vert_col.bottom_right;
		v_col.top_left = top_left - vert_col.top_left;
		v_col.top_right = top_right - vert_col.top_right;
		
		return v_col;
	}
	
	public VertexColour Multiply(float factor)
	{
		VertexColour v_col = new VertexColour();
		v_col.bottom_left = bottom_left * factor;
		v_col.bottom_right = bottom_right * factor;
		v_col.top_left = top_left * factor;
		v_col.top_right = top_right * factor;
		
		return v_col;
	}

	public void Clear()
	{
		top_left = Color.clear;
		top_right = Color.clear;
		bottom_right = Color.clear;
		bottom_left = Color.clear;
	}

	public void ClearAlpha()
	{
		top_left.a = 0;
		top_right.a = 0;
		bottom_left.a = 0;
		bottom_right.a = 0;
	}

	public bool Equals(VertexColour otherVertexCol)
	{
		return top_left == otherVertexCol.top_left && top_right == otherVertexCol.top_right && bottom_left == otherVertexCol.bottom_left && bottom_right == otherVertexCol.bottom_right;
	}
}