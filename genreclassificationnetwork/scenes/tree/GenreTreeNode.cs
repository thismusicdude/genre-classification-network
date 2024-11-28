using Godot;
using System;


namespace MusicGenreTreeView {
	public partial class GenreTreeNode : Node3D
	{
		private string m_genreTitle = "NONE";
		private SphereMesh m_sphere = new();

		public GenreTreeNode() {
			LoadSphereMesh();
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Label3D genreLabel = GetNode<Label3D>("GenreTitleLabel");
			genreLabel.Text = m_genreTitle;
		}

		private void AdjustTitleHeight(){
			throw new NotImplementedException("adjusting the height of the title in relation of the sphere size is not implemented");
		}

		private void LoadSphereMesh(){
			MeshInstance3D mesh = new();
			mesh.Mesh = m_sphere;
			AddChild(mesh);
		}

		public void SetGenreTitle(string title){
			this.m_genreTitle = title;
		}

		/// <summary>
		/// sets the size of the sphere
		/// </summary>
		/// <param name="size">sets size from 0.0 to 1.0</param>
		public void SetSize(float size){
			
			m_sphere.RadialSegments = 12;
			m_sphere.Rings = 6;

			float startRadius = 0.1f;
			float endradius = 1.0f;

			// make sure that size is between 0.0f and 1.0f
			size = Mathf.Clamp(size, 0.0f, 1.0f);

			// interpolate radius
			float newRadius = Mathf.Lerp(startRadius, endradius, size);

			m_sphere.Radius = newRadius;
			m_sphere.Height = newRadius * 2;

			// TODO: Adjust Label height
		}

		public void SetSphereColor(Color color){
			// TODO: Find a way to set the material of the MeshSphere into a specific color
			// throw new NotImplementedException("Color selection for sphere is not implemented yet");
		}

		public void HideTextLabel(bool is_hidden){
			if(is_hidden){
				GetNode<Label3D>("GenreTitleLabel").Hide();
			} else { 
				GetNode<Label3D>("GenreTitleLabel").Show();
			}
		}

		public void SetSphereSegments(int segments){
			// TODO: Do we need that?
		}
	}
}
