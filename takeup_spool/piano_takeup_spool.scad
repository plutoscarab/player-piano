$fn = 64;

include <MCAD/involute_gears.scad>


// Basic dimensions of 11 1/4" wide 3/4" diameter spool

inch = 25.4;
mm = 1.0;

length = 11.25 * inch;
diameter = 0.75 * inch;
endcap_diameter = 2 * inch;
endcap_thickness = 3 * mm;


// Skate bearing dimensions

bearing_outer_diam = 22 * mm;
bearing_inner_diam = 8 * mm;
bearing_depth = 7 * mm;


// Dissection and joints

translate([-75, 0, 0])
#spool();

translate([2 * diameter, 0, 0])
intersection()
{
    spool();
    midsection();
}

rotate([0, 0, 360 * $t])
difference()
{
    spool();
    midsection();
}


module midsection()
{
    difference()
    {
        dis = length / 6;

        union()
        {
            cube([2 * diameter, 2 * diameter, 2 * dis], center = true);
            
            translate([0, 0, dis - .1])
            linear_extrude(diameter / 3)
            circle(diameter / 3, $fn = 6);
        }
        
        translate([0, 0, -dis])
        linear_extrude(diameter / 3 + .2)
        circle(diameter / 3 + .05, $fn = 6);
    }
}



endcap_radius = endcap_diameter / 2;

module endcap()
{
    translate([0, 0, length / 2])
    {
        rotate_extrude(convexity = 4)
        translate([endcap_radius - endcap_thickness / 2, endcap_thickness / 2, 0])
        circle(r = endcap_thickness / 2);

        cylinder(r = endcap_radius - endcap_thickness / 2, h = endcap_thickness);
    }
}


module housing()
{
    housing_thickness = 4;

    color("green")
    difference()
    {
        cylinder(r = bearing_outer_diam / 2 + housing_thickness, h = bearing_depth);
        
        translate([0, 0, -.1])
        cylinder(r = bearing_outer_diam / 2 + .05, h = bearing_depth + .2);
    }
}


module spool()
{
    // Shaft

    radius = diameter / 2;
    hook_recess_depth = 6 * mm;
    hook_recess_radius = 16 * mm;

    difference()
    {
        cylinder(r = radius, h = length + .1, center = true);
        
        // Hook recess
        translate([hook_recess_radius + radius - hook_recess_depth, 0, 0])
        rotate([90, 0, 0])
        cylinder(r = hook_recess_radius, h = diameter, center = true);
    }


    // End caps

    endcap();
    mirror([0, 0, 1]) endcap();


    // Hook

    hook_thickness = 3.5 * mm;
    hook_radius = radius * .95  - hook_thickness / 2;

    rotate([0, 0, 10])
    {
        intersection()
        {
            rotate_extrude(convexity = 4)
            translate([hook_radius, 0, 0])
            circle(hook_thickness / 2);
            
            translate([0, 0, -radius])
            cube([diameter, diameter, diameter]);
        }

        translate([hook_radius, 0, 0])
        rotate([90, 0, 0])
        cylinder(h = hook_thickness / 2, r1 = hook_thickness / 2, r2 = 0);
    }


    // Gear

    gear_teeth = 17;
    gear_pitch = 450;
    gear_thickness = bearing_depth - .1;

    translate([0, 0, length / 2 + endcap_thickness])
    gear(
        number_of_teeth = gear_teeth,
        circular_pitch = gear_pitch,
        pressure_angle = 20,
        hub_diameter = 0,
        bore_diameter = 0,
        gear_thickness = .1,
        rim_width = 3.6,
        rim_thickness = gear_thickness);


    // Bearing housings
    
    translate([0, 0, -length / 2 - bearing_depth - endcap_thickness + .1])
    housing();
    
    translate([0, 0, length / 2 + endcap_thickness - .1])
    housing();
}
