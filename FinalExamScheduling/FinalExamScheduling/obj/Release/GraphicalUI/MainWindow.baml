   M S B A M L   `   `   `  ���� e  aFinalExamScheduling_TabuSearch_GraphicalUI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:FinalExamSchedulingFinalExamScheduling  $    FinalExamScheduling.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     3local!clr-namespace:FinalExamScheduling     $
��1300��$	��800��$
��1300��$	��800��  � Title$1  +TabuSearh Algorithm - Final Exam Scheduling��.���  � ColumnDefinitions �� $��2* ��� $��2* ��� $��9* � � RowDefinitions �� $��1* ��� $	��20* �.+�� $��0��$��0�� �� $��1* ��� $��1* �.+��� -    ��Name$ 	runButton��$	��Run��$��Stretch=�$��Stretch=�$��0��$��8q�$
��Auto���� -   $ abortButton��$��Abort��$��Stretch=�$��Stretch=�$��1��{�.  $��8q�� $��1��$��0��$��5q�.+��� -    ��Name$ resultGroupBox��$��Run results:��.���� -    ��Name$ 	resultBox��� $��1��$��1��$��5q�.+��� -   $ statBox��$��Brief result statistics��.���  �� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $	��15* �.+��� -    ��Name$ avgTitleLabel��$��Last run's average:��$��0���� -   $ avgLabel��$��-��$
��Left=�$��1���� -   $ minTitleLabel��$��Last run's minimum:��$��2���� -	   $ minLabel��$��-��$��3���� -
   $ feasiblePercentageTitle��$%��Feasible solutions in last run:��$��4���� -   $ feasiblePercentageLabel��$��-��$��5��� $��1��$��2��$��5q�.+��� -   $ parameterBox��$��Algorithm parameters��.��� $��5q� �� $��4* ��� $��4* ��� $��3* ��� $��2* � �� $��2* ��� $��4* ��� $��4* ��� $��5* ��� $��5* �.+��� -   $ modeBox��$��Mode selection��$��0��$��0��$��5q�.��� .+��� -    ��Name$ modeSelector��$
��Left=�$��Center=�$��15,0,0,0q� �� Text$ Runner Mode��$	��195��$��22��.@��� 	 �� 
IsSelected	 . $��Tandem���� $��Random���� $��	Heuristic���� -   $ basicSwitchesBox��$��Main switches��$��1��$��0��$��5q�.���  �� $��1* ��� $��1* ��� $��1* �.+��� -   
 ��Name$
 softConstCheckBox��$��Optimize soft constraints��$
��Left=�$��Center=�$��15,0,0,0q�$��0�� d� 	IsChecked$
 TrueY��� -   $
 restartCheckBox��$��Auto restart algorithm��$
��Left=�$��Center=�$��15,0,0,0q�$��1��$
 TrueY��� -   $
 hardFirstCheckBox��$��Fix hard violations first��$
��Left=�$��Center=�$��15,0,0,0q�$��2��$
 TrueY��� -   $ writeOutLimitBox��$��Writing to Excel - Limit��$��2��$��0��$��5q�.���  �� $��1* ��� $��1* ��� $��1* �.+��� -   $ writeOutTitleLabel��$)��#Write result to Excel below points:��$��0��$��Center=�$��5,0,0,0q��� -    ��Name$ writeOutLimitInput��$
��Left=�$��Center=�$��15,0,0,0q� �� TextWrapping$
 Wrap=�$	��500��$��1��$	��120���� -   $
 writeOutCheckBox��$!��Write every result to Excel��$��2��$��Center=�$��15,0,0,0q��� -   $ tabuParametersBox��$��Tabu parameters��$��3��$��0��$��2��$��5q�.���  �� $��1* ��� $��1* �.+��� -   $ randomSearchParametersBox��$��Random search��$��0��$��5q�.���  �� $��1* ��� $��1* ��� $��1* ��� $��1* �.+��� -   $ randomTabuLifeIterTitle��$��Tabu element TTL:��$��0��$��5,0,0,0q��� -   $ randomTTLInput��$
 Wrap=�$��10��$	��120��$��1��$
��Left=�$��Center=�$��15,0,0,0q��� -   $ randomTabuListLengthTitle��$��Tabu list length:��$��2��$��5,0,0,0q��� -   $ randomTabuListLengthInput��$
 Wrap=�$��10��$	��120��$��3��$
��Left=�$��Center=�$��15,0,0,0q��� -   $" heuristicSearchParametersBox��$��Heuristic search��$��1��$��5q�.���  �� $��1* ��� $��1* ��� $��1* ��� $��1* �.+��� -   $  heuristicTabuLifeIterTitle��$��Tabu element TTL:��$��0��$��5,0,0,0q��� -   $ heuristicTTLInput��$
 Wrap=�$��5��$	��120��$��1��$
��Left=�$��Center=�$��15,0,0,0q��� -    $" heuristicTabuListLengthTitle��$��Tabu list length:��$��2��$��5,0,0,0q��� -!   $" heuristicTabuListLengthInput��$
 Wrap=�$��5��$	��120��$��3��$
��Left=�$��Center=�$��15,0,0,0q��� -"   $ globalParametersBox��$!��Global algorithm parameters��$��0��$��1��$��3��$��5q�.���  �� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* �.+��� -#   $ generatedCandidatesTitle��$%��Number of generated neighbours:��$��0��$��5,0,0,0q��� -$   $ generatedCandidatesInput��$
 Wrap=�$��20��$	��120��$��1��$
��Left=�$��Center=�$��15,0,0,0q��� -%   $  allowedIdleIterationsTitle��$��Allowed idle iterations:��$��2��$��5,0,0,0q��� -&   $  allowedIdleIterationsInput��$
 Wrap=�$��10��$	��120��$��3��$
��Left=�$��Center=�$��15,0,0,0q��� -'   $( "maxFailedNeighbourGenerationsTitle��$%��Max failed generation attempts:��$��4��$��5,0,0,0q��� -(   $( "maxFailedNeighbourGenerationsInput��$
 Wrap=�$��5��$	��120��$��5��$
��Left=�$��Center=�$��15,0,0,0q��� -)   $ tandemIdleSwitchesTitle��$#��Allowed idle Tandem switches:��$��6��$��5,0,0,0q��� -*   $ tandemIdleSwitchesInput��$
 Wrap=�$��10��$	��120��$��7��$
��Left=�$��Center=�$��15,0,0,0q��� -+   $ targetScoreTitle��$��Target points:��$��8��$��5,0,0,0q��� -,   $ targetScoreInput��$
 Wrap=�$	��300��$	��120��$��9��$
��Left=�$��Center=�$��15,0,0,0q��� --   $ violationCountTitle��$'��!Violations to fix per generation:��$��10��$��5,0,0,0q��� -.   $ violationCountInput��$
 Wrap=�$��50��$	��120��$��11��$
��Left=�$��Center=�$��15,0,0,0q��� -/   $ 
shuffleBox��$��	Shuffling��$��0��$��2��$��2��$��5q�.���  �� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* ��� $��1* �.+��� -0   $ shufflePercentageTitle��$��Shuffle percentage:��$��0��$��5,0,0,0q��� -1   $ shufflePercentageInput��$
 Wrap=�$��20��$	��120��{�.  $��1��$
��Left=�$��Center=�$��15,0,0,0q��� -2   $ maxShufflesTitle��$��Max shuffle count:��$��2��$��5,0,0,0q��� -3   $ maxShufflesInput��$
 Wrap=�$��1��$	��120��{�.  $��3��$
��Left=�$��Center=�$��15,0,0,0q��� -4   $
 shuffleCheckBox��$��Allow shuffling��$��4��$
��Left=�$��Center=�$��15,0,0,0q�