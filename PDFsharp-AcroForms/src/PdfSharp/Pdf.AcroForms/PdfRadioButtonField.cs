#region PDFsharp - A .NET library for processing PDF
//
// Authors:
//   Stefan Lange
//
// Copyright (c) 2005-2017 empira Software GmbH, Cologne Area (Germany)
//
// http://www.pdfsharp.com
// http://sourceforge.net/projects/pdfsharp
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using PdfSharp.Pdf.Annotations;
using System;
using System.Collections.Generic;

namespace PdfSharp.Pdf.AcroForms
{
    /// <summary>
    /// Represents the radio button field.
    /// </summary>
    public sealed class PdfRadioButtonField : PdfButtonField
    {
        /// <summary>
        /// Initializes a new instance of PdfRadioButtonField.
        /// </summary>
        internal PdfRadioButtonField(PdfDocument document)
            : base(document)
        {
            _document = document;
        }

        internal PdfRadioButtonField(PdfDictionary dict)
            : base(dict)
        {
            if (!Elements.ContainsKey(Keys.Opt))
            {
                var array = new PdfArray(_document);
                foreach (var val in FieldValues)
                    array.Elements.Add(new PdfName(val));
                Elements.Add(Keys.Opt, array);
            }
        }

        /// <summary>
        /// Gets or sets the value of this field. This should be an item from the <see cref="FieldValues"/> list.
        /// </summary>
        public override PdfItem Value
        {
            get { return base.Value; }
            set
            {
                base.Value = value;
                var index = IndexInOptStrings(value.ToString());
                SelectedIndex = index;
            }
        }

        /// <summary>
        /// Gets the name of the Field-Appearances for the RadioButtons in the "checked" state. (unchecked value should be "/Off")
        /// Use this as the value to set the value for the whole RadioButton group.
        /// </summary>
        public IList<string> FieldValues
        {
            get
            {
                var values = new List<string>();
                for (var i = 0; i < Annotations.Elements.Count; i++)
                {
                    var widget = Annotations.Elements[i];
                    if (widget == null)
                        continue;

                    var ap = widget.Elements.GetDictionary(PdfAnnotation.Keys.AP);
                    if (ap != null)
                    {
                        foreach (var dictName in new[] { "/N", "/D" }) // try both
                        {
                            var found = false;
                            var n = ap.Elements.GetDictionary(dictName);
                            if (n != null)
                            {
                                foreach (var key in n.Elements.Keys)
                                {
                                    if (key != "/Off")
                                    {
                                        values.Add(key);
                                        found = true;
                                        break;
                                    }
                                }
                                if (found)
                                    break;
                            }
                        }
                    }
                }
                return values;
            }
        }

        /// <summary>
        /// Gets or sets the index of the selected radio button in a radio button group.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                string value = Elements.GetString(PdfAcroField.Keys.V);
                return IndexInOptStrings(value);
            }
            set
            {
                var opt = Elements.GetArray(Keys.Opt);
                if (opt != null)
                {
                    int count = opt.Elements.Count;
                    if (value < -1 || value >= count)
                        throw new ArgumentOutOfRangeException("value");
                    var name = value == -1 ? "/Off" : opt.Elements[value].ToString();
                    Elements.SetName(PdfAcroField.Keys.V, name);
                    // first, set all annotations to /Off
                    for (var i = 0; i < Annotations.Elements.Count; i++)
                    {
                        var widget = Annotations.Elements[i];
                        if (widget != null)
                            widget.Elements.SetName(PdfAnnotation.Keys.AS, "/Off");
                    }
                    if ((Flags & PdfAcroFieldFlags.RadiosInUnison) != 0)
                    {
                        // Then set all Widgets with the same Appearance to the checked state
                        for (var i = 0; i < Annotations.Elements.Count; i++)
                        {
                            var widget = Annotations.Elements[i];
                            if (name == opt.Elements[i].ToString() && widget != null)
                                widget.Elements.SetName(PdfAnnotation.Keys.AS, name);
                        }
                    }
                    else
                    {
                        if (value >= 0 && value < Annotations.Elements.Count)
                        {
                            var widget = Annotations.Elements[value];
                            if (widget != null)
                                widget.Elements.SetName(PdfAnnotation.Keys.AS, name);
                        }
                    }
                }
            }
        }

        private int IndexInOptStrings(string value)
        {
            var opt = Elements.GetArray(Keys.Opt);
            if (opt != null)
            {
                int count = opt.Elements.Count;
                for (int idx = 0; idx < count; idx++)
                {
                    PdfItem item = opt.Elements[idx];
                    if (item is PdfString)
                    {
                        if (item.ToString() == value)
                            return idx;
                    }
                }
            }
            return -1;
        }

        internal override void Flatten()
        {
            base.Flatten();

            for (var i = 0; i < Annotations.Elements.Count; i++)
            {
                var widget = Annotations.Elements[i];
                if (widget.Page != null)
                {
                    var appearance = widget.Elements.GetDictionary(PdfAnnotation.Keys.AP);
                    var selectedAppearance = widget.Elements.GetName(PdfAnnotation.Keys.AS);
                    if (appearance != null && selectedAppearance != null)
                    {
                        // /N -> Normal appearance, /R -> Rollover appearance, /D -> Down appearance
                        var apps = appearance.Elements.GetDictionary("/N");
                        if (apps != null)
                        {
                            var appSel = apps.Elements.GetDictionary(selectedAppearance);
                            if (appSel != null)
                            {
                                RenderContentStream(widget.Page, appSel.Stream, widget.Rectangle);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Predefined keys of this dictionary. 
        /// The description comes from PDF 1.4 Reference.
        /// </summary>
        public new class Keys : PdfButtonField.Keys
        {
            /// <summary>
            /// (Optional; inheritable; PDF 1.4) An array of text strings to be used in
            /// place of the V entries for the values of the widget annotations representing
            /// the individual radio buttons. Each element in the array represents
            /// the export value of the corresponding widget annotation in the
            /// Kids array of the radio button field.
            /// </summary>
            [KeyInfo(KeyType.Array | KeyType.Optional)]
            public const string Opt = "/Opt";

            /// <summary>
            /// Gets the KeysMeta for these keys.
            /// </summary>
            internal static DictionaryMeta Meta
            {
                get { return _meta ?? (_meta = CreateMeta(typeof(Keys))); }
            }
            static DictionaryMeta _meta;
        }

        /// <summary>
        /// Gets the KeysMeta of this dictionary type.
        /// </summary>
        internal override DictionaryMeta Meta
        {
            get { return Keys.Meta; }
        }
    }
}
