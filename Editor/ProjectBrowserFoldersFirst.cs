#if UNITY_EDITOR_OSX

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Kogane.Internal
{
    [InitializeOnLoad]
    internal static class ProjectBrowserFoldersFirst
    {
        private static readonly Assembly   ASSEMBLY                    = Assembly.GetAssembly( typeof( Editor ) );
        private static readonly Type       PROJECT_BROWSER_TYPE        = ASSEMBLY.GetType( "UnityEditor.ProjectBrowser" );
        private static readonly FieldInfo  PROJECT_BROWSERS_FIELD_INFO = PROJECT_BROWSER_TYPE.GetField( "s_ProjectBrowsers", BindingFlags.Static | BindingFlags.NonPublic );
        private static readonly MethodInfo RESET_VIEWES_METHOD_INFO    = PROJECT_BROWSER_TYPE.GetMethod( "ResetViews", BindingFlags.Instance | BindingFlags.NonPublic );

        static ProjectBrowserFoldersFirst()
        {
            EditorApplication.projectChanged         -= OnProjectChanged;
            EditorApplication.projectChanged         += OnProjectChanged;
            EditorApplication.playModeStateChanged   -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged   += OnPlayModeStateChanged;
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemOnGUI;
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
            foreach ( var x in ( IEnumerable )PROJECT_BROWSERS_FIELD_INFO.GetValue( PROJECT_BROWSER_TYPE ) )
            {
                SetFolderFirstForProjectWindow( x );

                RESET_VIEWES_METHOD_INFO.Invoke( x, Array.Empty<object>() );
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

            var assetTree = assetTreeField.GetValue( projectBrowser );

            if ( assetTree == null ) return;

            var data        = assetTree.GetType().GetRuntimeProperties().First( x => x.Name == "data" );
            var dataSource  = data.GetValue( assetTree );
            var folderFirst = dataSource.GetType().GetProperties().First( x => x.Name == "foldersFirst" );

            folderFirst.SetValue( dataSource, true );
        }

        private static void SetTwoColumnFolderFirst( object projectBrowser, FieldInfo fieldInfo )
        {
            if ( fieldInfo == null ) return;

            var listArea = fieldInfo.GetValue( projectBrowser );

            if ( listArea == null ) return;

            var folderFirst = listArea.GetType().GetProperties().First( x => x.Name == "foldersFirst" );

            folderFirst.SetValue( listArea, true );
        }
    }
}

#endif