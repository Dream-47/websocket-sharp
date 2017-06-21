#region License
/*
 * HttpRequestEventArgs.cs
 *
 * The MIT License
 *
 * Copyright (c) 2012-2015 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

using System;
using System.IO;
using System.Text;
using WebSocketSharp.Net;

namespace WebSocketSharp.Server
{
  /// <summary>
  /// Represents the event data for the HTTP request events of
  /// the <see cref="HttpServer"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///   An HTTP request event occurs when the <see cref="HttpServer"/>
  ///   receives an HTTP request.
  ///   </para>
  ///   <para>
  ///   You should access the <see cref="Request"/> property if you would
  ///   like to get the request data sent from a client.
  ///   </para>
  ///   <para>
  ///   And you should access the <see cref="Response"/> property if you would
  ///   like to get the response data to return to the client.
  ///   </para>
  /// </remarks>
  public class HttpRequestEventArgs : EventArgs
  {
    #region Private Fields

    private HttpListenerContext _context;
    private string              _rootPath;

    #endregion

    #region Internal Constructors

    internal HttpRequestEventArgs (HttpListenerContext context, string rootPath)
    {
      _context = context;
      _rootPath = rootPath;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the HTTP request data sent from a client.
    /// </summary>
    /// <value>
    /// A <see cref="HttpListenerRequest"/> that represents the request data.
    /// </value>
    public HttpListenerRequest Request {
      get {
        return _context.Request;
      }
    }

    /// <summary>
    /// Gets the HTTP response data to return to the client.
    /// </summary>
    /// <value>
    /// A <see cref="HttpListenerResponse"/> that represents the response data.
    /// </value>
    public HttpListenerResponse Response {
      get {
        return _context.Response;
      }
    }

    #endregion

    #region Private Methods

    private string createFilePath (string childPath)
    {
      childPath = childPath.TrimStart ('/', '\\');

      var buff = new StringBuilder (_rootPath, 32);
      if (_rootPath == "/" || _rootPath == "\\")
        buff.Append (childPath);
      else
        buff.AppendFormat ("/{0}", childPath);

      return buff.ToString ().Replace ('\\', '/');
    }

    private static bool tryReadFile (string path, out byte[] contents)
    {
      contents = null;

      if (!File.Exists (path))
        return false;

      try {
        contents = File.ReadAllBytes (path);
      }
      catch {
        return false;
      }

      return true;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Reads the file with the specified <paramref name="path"/> from
    /// the document folder of the <see cref="HttpServer"/>.
    /// </summary>
    /// <returns>
    ///   <para>
    ///   An array of <see cref="byte"/> or <see langword="null"/>
    ///   if not found.
    ///   </para>
    ///   <para>
    ///   That array receives the contents of the file.
    ///   </para>
    /// </returns>
    /// <param name="path">
    /// A <see cref="string"/> that represents a virtual path to
    /// the file to find from the document folder.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="path"/> is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is an invalid path.
    ///   </para>
    /// </exception>
    public byte[] ReadFile (string path)
    {
      if (path == null)
        throw new ArgumentNullException ("path");

      if (path.Length == 0)
        throw new ArgumentException ("An empty string.", "path");

      if (path.IndexOf (':') > -1)
        throw new ArgumentException ("It contains ':'.", "path");

      if (path.IndexOf ("..") > -1)
        throw new ArgumentException ("It contains '..'.", "path");

      if (path.IndexOf ("//") > -1)
        throw new ArgumentException ("It contains '//'.", "path");

      if (path.IndexOf ("\\\\") > -1)
        throw new ArgumentException ("It contains '\\\\'.", "path");

      byte[] contents;
      tryReadFile (createFilePath (path), out contents);

      return contents;
    }

    /// <summary>
    /// Tries to read a file with the specified <paramref name="path"/>
    /// from the document folder of the <see cref="HttpServer"/>.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the file could successfully be read;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <param name="path">
    /// A <see cref="string"/> that represents a virtual path to
    /// the file to read.
    /// </param>
    /// <param name="contents">
    ///   <para>
    ///   When this method returns, an array of <see cref="byte"/> or
    ///   <see langword="null"/> if the file could not be read.
    ///   </para>
    ///   <para>
    ///   That array receives the contents of the file.
    ///   </para>
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="path"/> is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is an invalid path.
    ///   </para>
    /// </exception>
    public bool TryReadFile (string path, out byte[] contents)
    {
      if (path == null)
        throw new ArgumentNullException ("path");

      if (path.Length == 0)
        throw new ArgumentException ("An empty string.", "path");

      if (path.IndexOf (':') > -1)
        throw new ArgumentException ("It contains ':'.", "path");

      if (path.IndexOf ("..") > -1)
        throw new ArgumentException ("It contains '..'.", "path");

      if (path.IndexOf ("//") > -1)
        throw new ArgumentException ("It contains '//'.", "path");

      if (path.IndexOf ("\\\\") > -1)
        throw new ArgumentException ("It contains '\\\\'.", "path");

      return tryReadFile (createFilePath (path), out contents);
    }

    #endregion
  }
}
