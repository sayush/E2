using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace E2Charts
{
   public class MapViewModel: IViewModel
    {
        private IDrawingManager _dm;

        public MapViewModel(IGraph sg, DataTable dt, StoryType storyType)
        {
            _dm = new MapDrawingManager(sg.GetLayout(), dt, storyType);
        }

        public void Draw()
        {
            _dm.Draw();
        }
    }
}
