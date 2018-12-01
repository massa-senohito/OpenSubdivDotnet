using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		static void Main( string[] args )
		{
			var exePath = AppDomain.CurrentDomain.BaseDirectory;
			var path = "mikuCsv.csv";
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

			using ( var refi = new Refiner( Vertice , faceGr , 3 ) )
			{

				refi.DoRefine( );
			}

		}
	}
}
