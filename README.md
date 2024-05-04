The purpose of this project is as a proof-of-concept: can C# leverage the analysis and statistics power of R (a similar project has already been done for Python).

The concept is simple: instead of writing new routines within the Epi Info software, leverage the vast resources of the R/Python language to do the heavy lifting for us.  If a researcher needs to run some analysis or create some vizualition not offered by Epi Info--it's not problem. They just take data already available in Epi Info and execute R (or Python) code to do what they need

In the current solution, data, which is a known data set to Epi Info (eColi) is imported in and we start with that data in a DataTable, just like if you were using Epi Info software. Then, you take execute some R script that does a specific analysis (cumulative frequency with confidence intervals). The analysis matches up with one of the analyses in the Epi Info manual.

The interesting part is, the table statistical results are completely created by R, and the output from R is consumed by C# and presented there (a proxy for Epi Info, which is written in C#).

The R output that is presented in this C# project.

Frequency Table for Sex:
-------------------------------------------------------------------------
Sex        Frequency    Cum Freq     Cum %        Wilson LCL   Wilson UCL
-------------------------------------------------------------------------
F-Female   186          186          51.81%       46.65        56.93
M-Male     173          359          100.00%      43.07        53.35
-------------------------------------------------------------------------
-------------------------------------------------------------------------
