﻿//  PlantUML Studio
//  Copyright 2016 Matthew Hamilton - matthamilton@live.com
//  Copyright 2010 Omar Al Zabir - http://omaralzabir.com/ (original author)
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Windows.Controls;

namespace PlantUmlStudio.View
{
    public partial class DiagramEditor : UserControl
    {
        public DiagramEditor()
        {
            InitializeComponent();

			ContentEditor.IsEnabledChanged += ContentEditor_IsEnabledChanged;
        }

		void ContentEditor_IsEnabledChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
		{
			// Maintain focus on the text editor.
			if ((bool)e.NewValue)
				Dispatcher.BeginInvoke(new Action(() => ContentEditor.Focus()));
		}
    }
}
