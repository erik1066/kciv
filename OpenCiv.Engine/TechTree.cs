using System.Collections.Generic;
using System.Linq;

namespace OpenCiv.Engine {
    public sealed class TechTree {

        private List<Tech> _techs = new List<Tech>(100);

        public TechTree()
        {
            Tech pottery = new Tech("Pottery", 25);
            Tech animal = new Tech("Animal Husbandry", 25);
            Tech mining = new Tech("Mining", 25);
            
            Tech irrigation = new Tech("Irrigation", 50);
            irrigation.AddPrereq(pottery);

            Tech writing = new Tech("Writing", 50);
            writing.AddPrereq(pottery);

            Tech archery = new Tech("Archery", 50);
            archery.AddPrereq(animal);

            Tech masonry = new Tech("Masonry", 80);
            masonry.AddPrereq(mining);

            Tech bronzeWorking = new Tech("Bronze Working", 80);
            bronzeWorking.AddPrereq(mining);

            Tech wheel = new Tech("Wheel", 80);
            wheel.AddPrereq(mining);

            Tech currency = new Tech("Currency", 120);
            currency.AddPrereq(writing);

            Tech horsebackRiding = new Tech("Horseback Riding", 120);
            horsebackRiding.AddPrereq(archery);

            Tech ironWorking = new Tech("Iron Working", 120);
            ironWorking.AddPrereq(bronzeWorking);

            Tech math = new Tech("Mathematics", 200);
            math.AddPrereq(currency);

            Tech construction = new Tech("Construction", 200);
            construction.AddPrereq(horsebackRiding);
            construction.AddPrereq(masonry);

            Tech engineering = new Tech("Engineering", 200);
            engineering.AddPrereq(wheel);

            Tech tactics = new Tech("Military Tactics", 275);
            tactics.AddPrereq(math);

            Tech guilds = new Tech("Guilds", 275);
            guilds.AddPrereq(currency);

            Tech civilService = new Tech("Civil Service", 275);
            civilService.AddPrereq(currency);
            civilService.AddPrereq(horsebackRiding);

            Tech apprenticeship = new Tech("Apprenticeship", 275);
            apprenticeship.AddPrereq(currency);
            apprenticeship.AddPrereq(horsebackRiding);

            Tech stirrups = new Tech("Stirrups", 360);
            stirrups.AddPrereq(horsebackRiding);

            Tech machinery = new Tech("Machinery", 275);
            machinery.AddPrereq(ironWorking);
            machinery.AddPrereq(engineering);
            machinery.AddPrereq(guilds);

            Tech education = new Tech("Education", 335);
            education.AddPrereq(apprenticeship);
            education.AddPrereq(math);

            Tech militaryEngineering = new Tech("Military Engineering", 335);
            militaryEngineering.AddPrereq(construction);

            Tech castles = new Tech("Castles", 390);
            castles.AddPrereq(construction);

            Tech metalCasting = new Tech("Metal Casting", 300);
            metalCasting.AddPrereq(engineering);
            metalCasting.AddPrereq(ironWorking);

            Tech steel = new Tech("Steel", 485);
            steel.AddPrereq(metalCasting);
            steel.AddPrereq(ironWorking);

            Tech physics = new Tech("Physics", 485);
            physics.AddPrereq(metalCasting);

            Tech chivalry = new Tech("Chivalry", 485);
            chivalry.AddPrereq(civilService);
            chivalry.AddPrereq(guilds);
            chivalry.AddPrereq(stirrups);

            Tech gunpowder = new Tech("Gunpowder", 780);
            gunpowder.AddPrereq(steel);
            gunpowder.AddPrereq(physics);

            Tech printingPress = new Tech("Printing Press", 780);
            printingPress.AddPrereq(machinery);
            printingPress.AddPrereq(physics);
            printingPress.AddPrereq(chivalry);

            Tech banking = new Tech("Banking", 780);
            banking.AddPrereq(education);
            banking.AddPrereq(chivalry);

            Tech economics = new Tech("Economics", 1150);
            economics.AddPrereq(printingPress);
            economics.AddPrereq(banking);

            Tech metallurgy = new Tech("Metallurgy", 1150);
            metallurgy.AddPrereq(gunpowder);

            Tech industrialization = new Tech("Industrialization", 1600);
            industrialization.AddPrereq(economics);

            _techs.Add(mining);
            _techs.Add(animal);
            _techs.Add(pottery);
            _techs.Add(irrigation);
            _techs.Add(writing);
            _techs.Add(archery);
            _techs.Add(masonry);
            _techs.Add(bronzeWorking);
            _techs.Add(wheel);
            _techs.Add(currency);
            _techs.Add(horsebackRiding);
            _techs.Add(ironWorking);
            _techs.Add(math);
            _techs.Add(construction);
            _techs.Add(engineering);
            _techs.Add(tactics);

            _techs.Add(guilds);
            _techs.Add(civilService);

            _techs.Add(apprenticeship);
            _techs.Add(stirrups);
            _techs.Add(machinery);
            _techs.Add(education);
            _techs.Add(militaryEngineering);
            _techs.Add(castles);

            _techs.Add(metalCasting);
            _techs.Add(steel);
            _techs.Add(physics);
            _techs.Add(chivalry);
            _techs.Add(gunpowder);
            _techs.Add(printingPress);
            _techs.Add(banking);
            _techs.Add(economics);

            _techs.Add(metallurgy);

            _techs.Add(industrialization);

            // sanity check
            foreach (var tech in _techs.Where(t => t.Prereqs.Count > 0)) {
                foreach(var prereq in tech.Prereqs) {
                    if (!_techs.Contains(prereq)) {
                        throw new System.InvalidOperationException("Technology prereq not found!");
                    }
                }
            }
        }

        public IList<Tech> Technologies
        {
            get
            {
                return _techs.AsReadOnly();
            }
        }

        public Tech GetNext(Civilization civ)
        {
            foreach(var tech in _techs)
            {
                if (civ.Techs.Contains(tech))
                {
                    continue; // civ has this tech
                }

                bool hasAllPrereqs = true;
                foreach(var prereq in tech.Prereqs)
                {
                    if (!civ.Techs.Contains(prereq))
                    {
                        hasAllPrereqs = false;
                        break;
                    }
                }

                if (hasAllPrereqs)
                {
                    return tech;
                }
            }

            return null;
        }
    }
}