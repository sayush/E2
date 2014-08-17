using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using E2Data;
using Direct2D.Interop;
using Direct2D;
using System.Windows.Controls;
using System.Data;
using System.Windows;

namespace E2Charts
{
    public class StoryViewModel: IViewModel
    {
        private IDrawingManager _dm;

        public StoryViewModel(IGraph sg, DataTable dt, StoryType storyType)
        {
            if (storyType == StoryType.TIMELINE)
            {
                _dm = new TimelineDrawingManager(sg.GetLayout(), dt, storyType);
            }
            else if (storyType == StoryType.STORYGRAPH || storyType == StoryType.STORYLINES || storyType == StoryType.STORYLINESWU)
            {
                _dm = new StoryDrawingManager(sg.GetLayout(), dt, storyType);
            }
        }

        public StoryViewModel(IGraph sg, DataTable dt, StoryType storyType, ForceParameters f)
        {
            _dm = new StoryDrawingManager(sg.GetLayout(), dt, storyType, f);
        }

        public void Draw()
        {
            _dm.Draw();
        }
    }
}
