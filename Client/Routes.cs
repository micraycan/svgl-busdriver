using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Routes : BaseScript
    {
        private static readonly Random rnd = new Random();

        public static List<Route> routes = new List<Route>
        {
            new Route(
                new Vector3(306.91f, -767.11f, 29.24f),
                new List<Vector3>
                {
                    new Vector3(303.79f, -766.18f, 28.31f), 
                    new Vector3(304.03f, -765.52f, 28.31f),
                    new Vector3(304.32f, -764.59f, 28.31f),
                    new Vector3(304.80f, -763.81f, 28.31f)
                }, 259 // 259 heading
                ),
            new Route(
                new Vector3(115.19f, -784.55f, 31.31f),
                new List<Vector3>
                {
                    new Vector3(113.85f, -781.37f, 30.41f), 
                    new Vector3(115.17f, -781.70f, 30.41f),
                    new Vector3(116.59f, -782.34f, 30.41f)
                }, 170 // 170 heading
                ),
            new Route(
                new Vector3(-110.08f, -1685.42f, 28.31f),
                new List<Vector3>
                {
                    new Vector3(-110.79f, -1687.16f, 28.31f),
                    new Vector3(-110.52f, -1686.35f, 28.31f),
                    new Vector3(-109.83f, -1685.55f, 28.31f),
                    new Vector3(-109.11f, -1684.65f, 28.31f)
                }, 240 // 240 heading
                ),
            new Route(
                new Vector3(-558.83f, -848.32f, 27.51f),
                new List<Vector3>
                {
                    new Vector3(-556.58f, -848.88f, 26.74f),
                    new Vector3(-557.79f, -849.03f, 26.62f),
                    new Vector3(-559.14f, -848.97f, 26.48f),
                    new Vector3(-560.28f, -848.36f, 26.36f)
                }, 7 // 7 heading
                ),
            new Route(
                new Vector3(-505.47f, -670.59f, 33.1f),
                new List<Vector3>
                {
                    new Vector3(-503.52f, -670.75f, 32.08f),
                    new Vector3(-504.52f, -671.01f, 32.09f),
                    new Vector3(-505.50f, -670.73f, 32.10f),
                    new Vector3(-503.52f, -670.73f, 32.12f)
                }, 0 // 0 heading
                ),
            new Route(
                new Vector3(-248.14f, -713.4f, 33.54f),
                new List<Vector3>
                {
                    new Vector3(-248.43f, -715.01f, 32.52f),
                    new Vector3(-248.19f, -713.83f, 32.54f),
                    new Vector3(-247.86f, -712.93f, 32.55f),
                    new Vector3(-247.44f, -712.10f, 32.56f)
                }, 244 // 244 heading
                ),
            new Route(
                new Vector3(-250.16f, -887.1f, 30.62f),
                new List<Vector3>
                {
                    new Vector3(-248.30f, -888.01f, 29.55f),
                    new Vector3(-249.34f, -887.33f, 29.59f),
                    new Vector3(-250.64f, -886.79f, 29.64f),
                    new Vector3(-251.85f, -886.35f, 29.68f),
                    new Vector3(-253.40f, -886.12f, 29.73f)
                }, 352 // 352 heading
                ),
            new Route(
                new Vector3(-1171.12f, -1473.49f, 4.38f),
                new List<Vector3>
                {
                    new Vector3(-1169.78f, -1475.17f, 3.38f), 
                    new Vector3(-1170.45f, -1474.29f, 3.38f),
                    new Vector3(-1171.24f, -1473.21f, 3.38f),
                    new Vector3(-1171.47f, -1471.63f, 3.38f)
                }, 326 // 326 heading
                ),
        };

        public static Stack<Route> GetRoute()
        {
            List<Route> shuffledRoutes = new List<Route>(routes);
            int n = shuffledRoutes.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                Route temp = shuffledRoutes[i];
                shuffledRoutes[i] = shuffledRoutes[j];
                shuffledRoutes[j] = temp;
            }

            int numberOfRoutesToReturn = rnd.Next(2, n + 1);
            List<Route> randomRoutes = shuffledRoutes.Take(numberOfRoutesToReturn).ToList();
            Stack<Route> routeStack = new Stack<Route>();

            foreach (Route route in randomRoutes)
            {
                routeStack.Push(route);
            }

            return routeStack;
        }
    }

    public class Route
    {
        public Vector3 Location { get; private set; }
        public List<Vector3> NPCSpawns { get; private set; }
        public int Heading { get; private set; }

        public Route(Vector3 location, List<Vector3> npcSpawns, int heading)
        {
            Location = location;
            NPCSpawns = npcSpawns;
            Heading = heading;
        }
    }

    public static class Pedestrians
    {
        private static List<string> npcModels = new List<string>
        {
            "a_f_m_skidrow_01",
            "a_f_m_soucentmc_01",
            "a_f_m_soucent_01",
            "a_f_m_soucent_02",
            "a_f_m_tourist_01",
            "a_f_m_trampbeac_01",
            "a_f_m_tramp_01",
            "a_f_o_genstreet_01",
            "a_f_o_indian_01",
            "a_f_o_ktown_01",
            "a_f_o_salton_01",
            "a_f_o_soucent_01",
            "a_f_o_soucent_02",
            "a_f_y_beach_01",
            "a_f_y_bevhills_01",
            "a_f_y_bevhills_02",
            "a_f_y_bevhills_03",
            "a_f_y_bevhills_04",
            "a_f_y_business_01",
            "a_f_y_business_02",
            "a_f_y_business_03",
            "a_f_y_business_04",
            "a_f_y_eastsa_01",
            "a_f_y_eastsa_02",
            "a_f_y_eastsa_03",
            "a_f_y_epsilon_01",
            "a_f_y_fitness_01",
            "a_f_y_fitness_02",
            "a_f_y_genhot_01",
            "a_f_y_golfer_01",
            "a_f_y_hiker_01",
            "a_f_y_hipster_01",
            "a_f_y_hipster_02",
            "a_f_y_hipster_03",
            "a_f_y_hipster_04",
            "a_f_y_indian_01",
            "a_f_y_juggalo_01",
            "a_f_y_runner_01",
            "a_f_y_rurmeth_01",
            "a_f_y_scdressy_01",
            "a_f_y_skater_01",
            "a_f_y_soucent_01",
            "a_f_y_soucent_02",
            "a_f_y_soucent_03",
            "a_f_y_tennis_01",
            "a_f_y_tourist_01",
            "a_f_y_tourist_02",
            "a_f_y_vinewood_01",
            "a_f_y_vinewood_02",
            "a_f_y_vinewood_03",
            "a_f_y_vinewood_04",
            "a_f_y_yoga_01",
            "g_f_y_ballas_01",
		    "ig_barry",
            "ig_bestmen",
            "ig_beverly",
            "ig_car3guy1",
            "ig_car3guy2",
            "ig_casey",
            "ig_chef",
            "ig_chengsr",
            "ig_chrisformage",
            "ig_clay",
            "ig_claypain",
            "ig_cletus",
            "ig_dale",
            "ig_dreyfuss",
            "ig_fbisuit_01",
            "ig_floyd",
            "ig_groom",
            "ig_hao",
            "ig_hunter",
            "csb_prolsec",
            "ig_joeminuteman",
            "ig_josef",
            "ig_josh",
            "ig_lamardavis",
            "ig_lazlow",
            "ig_lestercrest",
            "ig_lifeinvad_01",
            "ig_lifeinvad_02",
            "ig_manuel",
            "ig_milton",
            "ig_mrk",
            "ig_nervousron",
            "ig_nigel",
            "ig_old_man1a",
            "ig_old_man2",
            "ig_oneil",
            "ig_orleans",
            "ig_ortega",
            "ig_paper",
            "ig_priest",
            "ig_prolsec_02",
            "ig_ramp_gang",
            "ig_ramp_hic",
            "ig_ramp_hipster",
            "ig_ramp_mex",
            "ig_roccopelosi",
            "ig_russiandrunk",
            "ig_siemonyetarian",
            "ig_solomon",
            "ig_stevehains",
            "ig_stretch",
            "ig_talina",
            "ig_taocheng",
            "ig_taostranslator",
            "ig_tenniscoach",
            "ig_terry",
            "ig_tomepsilon",
            "ig_tylerdix",
            "ig_wade",
            "ig_zimbor",
            "s_m_m_paramedic_01",
            "a_m_m_afriamer_01",
            "a_m_m_beach_01",
            "a_m_m_beach_02",
            "a_m_m_bevhills_01",
            "a_m_m_bevhills_02",
            "a_m_m_business_01",
            "a_m_m_eastsa_01",
            "a_m_m_eastsa_02",
            "a_m_m_farmer_01",
            "a_m_m_fatlatin_01",
            "a_m_m_genfat_01",
            "a_m_m_genfat_02",
            "a_m_m_golfer_01",
            "a_m_m_hasjew_01",
            "a_m_m_hillbilly_01",
            "a_m_m_hillbilly_02",
            "a_m_m_indian_01",
            "a_m_m_ktown_01",
            "a_m_m_malibu_01",
            "a_m_m_mexcntry_01",
            "a_m_m_mexlabor_01",
            "a_m_m_og_boss_01",
            "a_m_m_paparazzi_01",
            "a_m_m_polynesian_01",
            "a_m_m_prolhost_01",
            "a_m_m_rurmeth_01",
        };

        private static readonly Random rnd = new Random();

        public static string GetRandomNpcModel()
        {
            int randomIndex = rnd.Next(npcModels.Count);
            return npcModels[randomIndex];
        }
    }
}
