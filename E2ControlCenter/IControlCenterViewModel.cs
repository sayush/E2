using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using E2Charts;

namespace E2ControlCenter
{
    public enum OptionWindows {COLORPICKER};
    public interface IControlCenterViewModel
    {
        void FeedData(DataTable d, bool b);
        bool GetSaveState();
        bool GetDataState();

        void LoadGraph(E2Charts.StoryType storyType);
        void LoadOptions(OptionWindows w);
    }
}
