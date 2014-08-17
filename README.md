E2: Exploring Events
==

As the name suggests, its a visualization tool for Exploring Events built around Storygraph. Storygraph is a new way of visualizing spatio-temporal datasets developed by me for my Ph.D. dissertation. You can read more about Storygraph and related publications from my website. 

Over the years, i've come up with many versions, learned from my mistakes and tried to create a better and a more extendable piece of code. Sorry I'm still working on the documentation. 

-- I'm looking for collaborators for re-writing it in open source platform/language, if you want to get involved please let me know. ashrestha2 at cs dot gsu dot edu

Platforms
==

Windows 7, 8 (I havent tested it on any other platforms).
Better if you have a graphics card. Else may take a long time to render the storygraphs. In that case, you may want to turn off the location lines.

Usage
==

The executable file is in side the E2/E2.ControlCenter/bin/Debug/E2.ControlCenter.exe
Clicking this will open a Window. Click open and select the csv file.

The csv file must contain the following as the first line:

Lat,Lng,Date,Color,Label

The lines following that should contain the associated data eg.

40.32224,-73.92,9/11/2001 8:00:00,FFFF0726,"South St."

The first two are latitude and longitude
Date is the timestamp
Color signifies what color would you want the point to be
Label is the label for that point (mostly for legend)

When the data is successfully imported, the storygraph button will be highlighted. Clicking it will generate the storygraph. 

For Developers
==

This code was written in .Net and compiled using Visual Studio 2010.
The Storygraph rendering is using managed Direct2d since I work with datasets containing more than 50K points and using WPF was really really slow (I dont know if i messed up something). 

E2.Data handles the data import (seperate project because i want to add db later)
E2.Graph handles all the graphs (storygraph and storylines for now)
E2.ControlCenter for all the interactivity.

Thanks to
==

My wife Kebina, my professors Dr Ying Zhu (Computer Science), Dr Ben Miller (English and Communication) and Dr Yi Zhao (Mathematics) in Georgia State Univ.

Samuel Cragg (http://www.codeproject.com/script/Membership/View.aspx?mid=7241282) - I borrowed his Direct2D code for rendering the Storygraph.


Future Works
==

There's more to add in the control center like the interactivity buttons and map control.
Storygrams need to be added.
Please be sure to check out the page next time.






