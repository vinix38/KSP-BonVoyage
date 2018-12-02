using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BonVoyage
{
    class BonVoyageScenario : ScenarioModule
    {
        public static BonVoyageScenario Instance { get; private set; }

        private ConfigNode scenarioNode;


        /// <summary>
        /// Constructor
        /// </summary>
        public BonVoyageScenario()
        {
            Instance = this;
        }


        /// <summary>
        /// Load data
        /// </summary>
        /// <param name="gameNode"></param>
        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);
            scenarioNode = gameNode;
        }


        /// <summary>
        /// Save data
        /// </summary>
        /// <param name="gameNode"></param>
        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);
            SaveScenario(gameNode);
        }


        /// <summary>
        /// Save scenario details for each vessel with BV controller
        /// </summary>
        /// <param name="node"></param>
        private void SaveScenario(ConfigNode gameNode)
        {
            gameNode.ClearNodes();

            var controllers = BonVoyage.Instance.BVControllers;
            int count = controllers.Count;
            for (int i = 0; i < count; i++)
            {
                if (controllers[i].vessel != null)
                {
                    ConfigNode controllerNode = new ConfigNode("CONTROLLER");
                    controllerNode.AddValue("vesselId", controllers[i].vessel.id);

                    ConfigNode subNode = new ConfigNode("BATTERIES");
                    subNode.AddValue("useBatteries", controllers[i].batteries.UseBatteries);
                    subNode.AddValue("maxUsedEC", controllers[i].batteries.MaxUsedEC);
                    subNode.AddValue("ecPerSecondConsumed", controllers[i].batteries.ECPerSecondConsumed);
                    subNode.AddValue("ecPerSecondGenerated", controllers[i].batteries.ECPerSecondGenerated);
                    subNode.AddValue("currentEC", controllers[i].batteries.CurrentEC);
                    controllerNode.AddNode(subNode);

                    gameNode.AddNode(controllerNode);
                }
            }
        }


        /// <summary>
        /// Load scenario details for each vessel with BV controller
        /// </summary>
        public void LoadScenario()
        {
            if (scenarioNode != null)
            {
                var controllers = BonVoyage.Instance.BVControllers;
                int count = controllers.Count;
                for (int i = 0; i < count; i++)
                {
                    if(controllers[i].vessel != null)
                    {
                        ConfigNode controllerNode = scenarioNode.GetNode("CONTROLLER", "vesselId", controllers[i].vessel.id.ToString());
                        if (controllerNode != null)
                        {
                            ConfigNode subNode = controllerNode.GetNode("BATTERIES");
                            if (subNode != null)
                            {
                                controllers[i].batteries.UseBatteries = Convert.ToBoolean(subNode.GetValue("useBatteries"));
                                controllers[i].batteries.MaxUsedEC = Convert.ToDouble(subNode.GetValue("maxUsedEC"));
                                controllers[i].batteries.ECPerSecondConsumed = Convert.ToDouble(subNode.GetValue("ecPerSecondConsumed"));
                                controllers[i].batteries.ECPerSecondGenerated = Convert.ToDouble(subNode.GetValue("ecPerSecondGenerated"));
                                controllers[i].batteries.CurrentEC = Convert.ToDouble(subNode.GetValue("currentEC"));
                            }
                        }
                    }
                }
            }
        }

    }

}
