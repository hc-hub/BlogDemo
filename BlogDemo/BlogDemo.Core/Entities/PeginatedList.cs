using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace BlogDemo.Core.Entities
{
    public partial class PeginatedList : Component
    {
        public PeginatedList()
        {
            InitializeComponent();
        }

        public PeginatedList(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }
    }
}
