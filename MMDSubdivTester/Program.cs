using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MMDataIO.Pmx;
using SharpDX;
using SharpDXTest;

namespace MMDSubdivTester
{
	/*
	 * static float g_verts[8][3] = { { -0.5f, -0.5f,  0.5f },
                              {  0.5f, -0.5f,  0.5f },
                              { -0.5f,  0.5f,  0.5f },
                              {  0.5f,  0.5f,  0.5f },
                              { -0.5f,  0.5f, -0.5f },
                              {  0.5f,  0.5f, -0.5f },
                              { -0.5f, -0.5f, -0.5f },
                              {  0.5f, -0.5f, -0.5f } };

static int g_nverts = 8,
g_nfaces = 6;

static int g_vertsperface[6] = { 4, 4, 4, 4, 4, 4 };

static int g_vertIndices[24] = { 0, 1, 3, 2,
                                 2, 3, 5, 4,
                                 4, 5, 7, 6,
                                 6, 7, 1, 0,
                                 1, 7, 5, 3,
                                 6, 0, 2, 4 };
	 */

	class RefineMMD
	{
		List<Vector3> Vertexs = new List<Vector3>();
		List<int> Faces = new List<int>();
		List<Vector2> UVs = new List<Vector2>();

		List<int> VertOffset = new List<int>( );

		void Init()
		{
			Vertexs.Clear( );
			Faces.Clear( );
			UVs.Clear( );
			VertOffset.Clear( );
		}

		void OnVert( float x , float y , float z )
		{
			Vertexs.Add( new Vector3( x , y , z ) );
		}

		void OnFace( int v0 , int v1 , int v2 )
		{
			Faces.Add( v0 );
			Faces.Add( v1 );
			Faces.Add( v2 );
		}

		void OnUV( float u , float v )
		{
			UVs.Add( new Vector2( u , v ) );
		}

		static PmxVertexData MakeVert( Vector3 pos , Vector2 uv )
		{
			PmxVertexData pmxVertex = new PmxVertexData
			{
				Pos = pos ,
				Uv = uv ,
				BoneId = new int[] { 1 },
			};
			return pmxVertex;
		}

		List<PmxVertexData> CreateVert()
		{
			List<PmxVertexData> temp = new List<PmxVertexData>( );
			for ( int i = 0 ; i < Vertexs.Count ; i++ )
			{
				temp.Add( MakeVert( Vertexs[ i ] , UVs[ i ] ) );
			}
			return temp;
		}

		void refin( List<Vec3> vert , List<int> faceGr , List<Vector2> uv , AdaptiveOptionsCs option )
		{

			Init( );

			using ( var refi = new Refiner( vert , faceGr , 3 ) )
			{
				refi.AddChannel( new FVarChannelCs( faceGr , uv.Count ) );
				for ( int i = 0 ; i < uv.Count ; i++ )
				{
					refi.SetUV( uv[ i ].X , uv[ i ].Y );
				}
				refi.AddOption( option );
				refi.OnFace = OnFace;
				refi.OnUV = OnUV;
				refi.OnVert = OnVert;
				refi.DoRefine( );
			}
		}

		public void DebugRect(string path)
		{
			using ( FileStream sr = new FileStream( path , FileMode.Open , FileAccess.Read ) )
			{
				using ( BinaryReader bin = new BinaryReader( sr ) )
				{
					var pmxModelData = new PmxModelData( bin );
					//Path = path;
					//PmxModelData.Write( path + "ex.pmx" );
					var vert = pmxModelData.VertexArray.Select( v => v.Pos.ToV3( ) ).ToList( );
					var uv = pmxModelData.VertexArray.Select( v => v.Uv ).ToList( );
					List<int> faceGr = pmxModelData.VertexIndices.ToList( );
					PmxMaterialData[] materialArray = pmxModelData.MaterialArray;
					string parent = Directory.GetParent( path ).FullName;
					var option = new AdaptiveOptionsCs( 1 );
					//option.UseInfSharpPatch = true;
					//option.UseSingleCreasePatch = true;
					option.ConsiderFVarChannels = true;
#if false
					// far_tutorial_6 がパラメトリックにずらす
					// far_tutorial_3 uvなど
						refin( vert , faceGr,uv, option );


					pmxModelData.VertexArray = CreateVert( ).ToArray( );
					// VertexIndices と同じ
					materialArray[ 0 ].FaceCount = Faces.Count;
					// boneid と materialは仮
					pmxModelData.VertexIndices = Faces.ToArray( );
					pmxModelData.Write( path + "ex.pmx" );
#else
					int currentFaces = 0;
					int refinedVertsCount = 0;
					var refinedVertice = new List<PmxVertexData>( );
					int appendedVerticeCount = 0;
					var inds = new List<int>( );
					var refineTargets = new List<int> { 0 , 1 , 2 , 5 , 8 , 9
					};
					for ( int i = 0 ; i < materialArray.Length ; i++ )
					{
						int faces = materialArray[ i ].FaceCount;
						var havingFaces = faceGr.Range( currentFaces , faces ).ToList( );
						// Distinctすると面IDがばらばらになっている、正しい頂点順で拾うためにソート
						var uniqueFaces = havingFaces.Distinct( ).ToList( );
						uniqueFaces.Sort( );
						var matVertice = uniqueFaces.Select( ind => pmxModelData.VertexArray[ ind ] );
						// リファインしないならもとの頂点のまま入れる
						if ( !refineTargets.Contains( i ) )
						{
							currentFaces += faces;
							refinedVertice.AddRange( matVertice );
							// 合計して増えた頂点数分オフセットすれば
							inds.AddRange( havingFaces// );//
								.Select( x => x + appendedVerticeCount ) );
							refinedVertsCount += matVertice.Count( );
							continue;
						}
						var uvInMat = uniqueFaces.Select( ind => uv[ ind ] ).ToList( );
						refin( vert , havingFaces ,uvInMat, option );
						materialArray[ i ].FaceCount = Faces.Count;
						List<PmxVertexData> createdVert = CreateVert( );
						refinedVertice.AddRange( createdVert );
						inds.AddRange( Faces.Select( x => x + refinedVertsCount ) );
						refinedVertsCount += createdVert.Count;
						appendedVerticeCount += createdVert.Count - matVertice.Count( );
						currentFaces += faces ;

					}
					pmxModelData.VertexArray = refinedVertice.ToArray();
					// VertexIndices と同じ
					// boneid と materialは仮
					pmxModelData.VertexIndices = inds.ToArray( );
					pmxModelData.Write( path + "smo.pmx" );
#endif
				}
			}
		}
	}

	class Program
	{
		public static IEnumerable<Vec3> ParseCSV( IEnumerable<string> lines )
		{
			//; Vertex,頂点Index,位置_x,位置_y,位置_z,法線_x,法線_y,法線_z,エッジ倍率,UV_u,UV_v,追加UV1_x,追加UV1_y,追加UV1_z,追加UV1_w,追加UV2_x,追加UV2_y,追加UV2_z,追加UV2_w,追加UV3_x,追加UV3_y,追加UV3_z,追加UV3_w,追加UV4_x,追加UV4_y,追加UV4_z,追加UV4_w,ウェイト変形タイプ(0:BDEF1 / 1:BDEF2 / 2:BDEF4 / 3:SDEF / 4:QDEF),ウェイト1_ボーン名,ウェイト1_ウェイト値,ウェイト2_ボーン名,ウェイト2_ウェイト値,ウェイト3_ボーン名,ウェイト3_ウェイト値,ウェイト4_ボーン名,ウェイト4_ウェイト値,C_x,C_y,C_z,R0_x,R0_y,R0_z,R1_x,R1_y,R1_z
			//   Vertex,0,0.3916405,16.48059,-0.7562667,0.383015,0.4676141,-0.7966408,1,0.8393391,0.7603291,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,"上半身2",1,"",0,"",0,"",0,0,0,0,0,0,0,0,0,0

			foreach ( string item in lines )
			{
				var csv = item.Split( ',' );
				yield return new Vec3(
					csv[ 2 ].Float( ), csv[ 3 ].Float( ), csv[ 4 ].Float( ) );
			}
		}


		[STAThread]
		static void Main( string[] args )
		{
#if true
			RefineMMD refine = new RefineMMD();
			var path = "debugRect.pmx";//ex.pmx";
			OpenFileDialog dialog = new OpenFileDialog( );
			dialog.Filter = ".pmx|*.pmx";
			if ( dialog.ShowDialog( ) == DialogResult.OK )
			{
				path = dialog.FileName;
			}
			refine.DebugRect( path );
			//RefineMMD debugRef = new RefineMMD( );
			//debugRef.DebugRect( path + "ex.pmx" );
#else
			var exePath = AppDomain.CurrentDomain.BaseDirectory;
			var path = @"miku.csv";
			string[] lines = File.ReadAllLines( path );
			//;Face,親材質名,面Index,頂点Index1,頂点Index2,頂点Index3
			//Face,"スカート腕ヘドフォン",0,858,840,855
			// のように最初の文字列がVertexだと頂点 Faceだと面になる ;が最初に来るものは説明用のものなので弾く
			var gr = lines.GroupBy( l => l.Split( ',' )[ 0 ] );
			var gs = gr.Where( g => !g.Key.Contains( ";" ) ).ToDictionary( s => s.Key , g => g.ToList( ) );

			var Vertice = ParseCSV( gs[ "Vertex" ] ).ToList( );
			var faceGr = //.GroupBy( s => s.Split( ',' )[ 1 ] ).ToDictionary( s => s.Key , g => g.ToList( ) );
				Util.ParseFaceCSVAll( gs[ "Face" ] ).SelectMany(x=>x).ToList();

			var verts = new List<Vec3> {
				new Vec3( -0.5f , -0.5f , 0.5f ),
				new Vec3( 0.5f , -0.5f , 0.5f ),
				new Vec3( -0.5f , 0.5f , 0.5f ),
				new Vec3( 0.5f , 0.5f , 0.5f ),
				new Vec3( -0.5f , 0.5f , -0.5f ),
				new Vec3( 0.5f , 0.5f , -0.5f ),
				new Vec3( -0.5f , -0.5f , -0.5f ),
				new Vec3( 0.5f , -0.5f , -0.5f ),
			};

			var faces =	new List<int>
				{ 0, 1, 3, 2,
				 2, 3, 5, 4,
				 4, 5, 7, 6,
				 6, 7, 1, 0,
				 1, 7, 5, 3,
				 6, 0, 2, 4 };

			var option = new AdaptiveOptionsCs( 1 );
			option.UseInfSharpPatch = true;
			option.UseSingleCreasePatch = true;
			// far_tutorial_6 がパラメトリックにずらす
			// far_tutorial_3 uvなど
			using ( var refi = new Refiner( Vertice , faceGr , 3 ) )
			{
				refi.AddOption( option );
				refi.DoRefine( );
			}
#endif

		}
	}
}
