#if UNITY_EDITOR_OSX

using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UniProjectViewExtensionForMac
{
	[InitializeOnLoad]
	public static class ProjectViewExtensionForMac
	{
		static ProjectViewExtensionForMac()
		{
			EditorApplication.projectChanged         += OnProjectChanged;
			EditorApplication.playModeStateChanged   += OnPlayModeStateChanged;
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
		}

		private static void OnProjectChanged()
		{
			Refresh();
		}

		private static void OnPlayModeStateChanged( PlayModeStateChange obj )
		{
			Refresh();
		}

		private static void OnProjectWindowItemOnGUI( string guid, Rect selectionRect )
		{
			EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemOnGUI;
			Refresh();
		}

		private static void Refresh()
		{
			var assembly       = Assembly.GetAssembly( typeof( Editor ) );
			var projectBrowser = assembly.GetType( "UnityEditor.ProjectBrowser" );
			var fieldInfo      = projectBrowser.GetField( "s_ProjectBrowsers", BindingFlags.Static | BindingFlags.NonPublic );

			if ( fieldInfo == null ) return;

			var list = ( IEnumerable ) fieldInfo.GetValue( projectBrowser );

			foreach ( var n in list )
			{
				SetFolderFirstForProjectWindow( n );
			}
		}

		private static void SetFolderFirstForProjectWindow( object projectBrowser )
		{
			var fieldInfos = projectBrowser
					.GetType()
					.GetRuntimeFields()
				;

			foreach ( var fieldInfo in fieldInfos )
			{
				switch ( fieldInfo.Name )
				{
					case "m_AssetTree":
						SetOneColumnFolderFirst( projectBrowser, fieldInfo );
						break;

					case "m_ListArea":
						SetTwoColumnFolderFirst( projectBrowser, fieldInfo );
						break;
				}
			}
		}

		private static void SetOneColumnFolderFirst( object projectBrowser, FieldInfo assetTreeField )
		{
			if ( assetTreeField == null ) return;

			var assetTree   = assetTreeField.GetValue( projectBrowser );
			var data        = assetTree.GetType().GetRuntimeProperties().First( x => x.Name == "data" );
			var dataSource  = data.GetValue( assetTree );
			var folderFirst = dataSource.GetType().GetProperties().First( x => x.Name == "foldersFirst" );

			folderFirst.SetValue( dataSource, true );
		}

		private static void SetTwoColumnFolderFirst( object projectBrowser, FieldInfo fieldInfo )
		{
			if ( fieldInfo == null ) return;

			var listArea    = fieldInfo.GetValue( projectBrowser );
			var folderFirst = listArea.GetType().GetProperties().First( x => x.Name == "foldersFirst" );

			folderFirst.SetValue( listArea, true );
		}
	}
}

#endif