# MSHexaGuideGenerator
A simple C# tool for generating guide images for Maplestory's 6th job class progression

There are two steps to generating a graphic:
1. Update Design.json to use the correct skills, images, and text for your class.
2. Drop a csv with your class skills in the following format:
(Skill name that matches Design.json), (Level), (% fd gain)

The % fd gain is relative, so as long as all the skills are on the same scale you don't need to mince numbers.

There is some fuzzy grouping logic to reduce the number of skills that worked well with my data but could require tweaking if your data is significantly different.