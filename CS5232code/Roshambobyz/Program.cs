using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roshambobyz
{
    class Program
    {
       static List<GlobalState> gslist = new List<GlobalState>();
       static int iters = 45;
        static void Main(string[] args)
        {
            
            State init1 = new State(1, iters);
            State init2 = new State(1, iters);
            State init3 = new State(1, iters);
            State inith = new State(1, iters);
            Player p1 = new Player(new List<State> { init1, new State(2, iters), new State(3, iters), new State(4, iters) }, init1, false);
            //   p1.setProb_dist(new double[] { 1 / 3.0, 1 / 3.0, 1 / 3.0 });
            Player p2 = new Player(new List<State> { init2, new State(2, iters), new State(3, iters), new State(4, iters) }, init2, false);//byzantine
         //   p2.setProb_dist(new double[] { 1 / 3.0, 1 / 3.0, 1 / 3.0 });
            Player p3 = new Player(new List<State> { init3, new State(2, iters), new State(3, iters), new State(4, iters) }, init3, true);
            p3.setProb_dist(new double[] { 1 /5.0, 1 / 5.0, 3 / 5.0 });
            Player p1h = new Player(new List<State> { inith, new State(2,iters), new State(3,iters), new State(4,iters) }, inith, true);
            p1h.setProb_dist(new double[] { 1 / 5.0, 1 / 5.0, 3 / 5.0 });
            Player[] playlist = new Player[] { p1, p2, p3, p1h };


            //creates the local state machines for each player
            for (int p_num = 0; p_num < playlist.Count(); p_num++)
            {
                for (int i = 1; i < playlist[p_num].getStateList().Count; i++)
                {
                    playlist[p_num].getInit().addToNextList(playlist[p_num].getStateList().ElementAt(i));
                    if (playlist[p_num].isHonest())
                    {

                        playlist[p_num].getStateList().ElementAt(i).setProb(playlist[p_num].getProb_dist()[i - 1]);//set initial probabilities
                    }

                    playlist[p_num].getStateList().ElementAt(i).addToNextList(playlist[p_num].getInit());
                }


            }



            GlobalState initgstate = new GlobalState(init1, init2, init3,iters);
            GlobalState inithgstate = new GlobalState(inith, init2, init3,iters);
            gslist.Add(initgstate);//add the initial state to the global state list
            calcallutils(p1, p2, p3, initgstate);
            calcratExpPayoff(p1, p2, p3,initgstate);

           
            
        
      //      System.Diagnostics.Debug.WriteLine("state list length " + gslist.Count);
            gslist = new List<GlobalState>();
            gslist.Add(inithgstate);
            calcallutils(p1h, p2, p3, inithgstate);
            calchonExpPayoff(p1h, p2, p3, inithgstate);
          

        }

        private static void calcallutils(Player p1,Player p2,Player p3, GlobalState initgstate) {
            for (int i = 1; i < p1.getStateList().Count; i++)
            {
                for (int j = 1; j < p2.getStateList().Count; j++)
                {
                    for (int k = 1; k < p3.getStateList().Count; k++)
                    {
                        GlobalState gstate = new GlobalState(p1.getStateList().ElementAt(i), p2.getStateList().ElementAt(j), p3.getStateList().ElementAt(k),iters);
                        gslist.Add(gstate);
                        initgstate.getNextGstate().Add(gstate);
                        int id1 = p1.getStateList().ElementAt(i).getId();
                        int id2 = p2.getStateList().ElementAt(j).getId();
                        int id3 = p3.getStateList().ElementAt(k).getId();
                        id1 -= 2;
                        id2 -= 2;
                        id3 -= 2;
                        bool p1win, p2win, p3win;
                        p1win = (id2 == id3) && ((id1 > id2 && id1 - id2 == 1) || id2 - id1 == 2);
                        p2win = (id3 == id1) && ((id2 > id3 && id2 - id3 == 1) || id3 - id2 == 2);
                        p3win = (id1 == id2) && ((id3 > id1 && id3 - id1 == 1) || id1 - id3 == 2);

                        if (!p1win && !p2win && !p3win)//if no one wins, restart, go to init state
                        {
                            gstate.getNextGstate().Add(initgstate);

                        }
                        calculateUtility(gstate, id1, id2, id3);
                        //  System.Diagnostics.Debug.WriteLine(id1+" "+id2+" "+id3);
                    }
                }
            }
        
        }

        private static void calcratExpPayoff(Player p1,Player p2,Player p3,GlobalState initgstate) {
            
            double max = 0;
        /*    for (int i = 0; i < iters; i++)
            {
                foreach (State s in p1.getStateList())
                {
                    foreach (State ns in s.getNextList())
                    {
                        double newMax = calcExpStatePayoff(s, ns, p2.getStateList(), p3.getStateList(),iters);
                        if (max < newMax+ns.getExputil(i))
                        {
                            max = newMax+ns.getExputil(i);
                        }
                    }
                        s.setExputil(i+1,max);
                    System.Diagnostics.Debug.WriteLine("max = " + s.getExputil(i)+" "+s.getId());
                }
                System.Diagnostics.Debug.WriteLine(initgstate.getutilite(i + 1) + " Global utility");
            }
        */
            for (int i = 1; i < iters; i++)
            {
                foreach (State s in p1.getStateList())
                {
                 /*   foreach (State ns in s.getNextList())
                    {
                        double newMax = calcExpStatePayoff(s, ns, p2.getStateList(), p3.getStateList(), iters);
                        if (max < newMax + ns.getExputil(i))
                        {
                            max = newMax + ns.getExputil(i);
                        }
                    }*/
                 //   s.setExputil(i + 1, max);
                 //   System.Diagnostics.Debug.WriteLine("max = " + s.getExputil(i) + " " + s.getId());
                    calcExpStatePayoff(s, p2.getStateList(), p3.getStateList(), i);
                }
                System.Diagnostics.Debug.WriteLine(initgstate.getutilite(i) + " Global utility");
            }
        }
        private static void calchonExpPayoff(Player p1, Player p2, Player p3,GlobalState inithgstate) {
          // iters = 2;
            double newMax = 0;
            for (int i = 1; i < iters; i++)
            {
                foreach (State s in p1.getStateList())
                {
                    newMax = 0;//getting the expected value through all the next states
                    
                        newMax += calchonExpStatePayoff(s, p2.getStateList(), p3.getStateList(),i);
                    //    System.Diagnostics.Debug.WriteLine("nextstate prob honp " + calchonExpStatePayoff(s, p2.getStateList(), p3.getStateList(),iters));
                        
                    
                      //  s.setExputil(i+1,newMax);
                       
                   // System.Diagnostics.Debug.WriteLine("honmax = " + s.getExputil(i) + " "+s.getId());
                }
                System.Diagnostics.Debug.WriteLine(inithgstate.getutilite(i)+" honest Global utility");
            }
        
        }
        private static double calcExpStatePayoff(State s,  List<State> list1, List<State> list2,int ite)
        {
            double nextutil = 0;
            double currentutil = 0;
            
            //determines all the possible current global states given the current local state s 

            
            foreach (State s2 in list1)
            {
                foreach (State s3 in list2)
                {
                    //(s,s2,s3) is one such possible global state

                    if (getGlobalState(s, s2, s3) == null)
                        continue;
                    GlobalState gs = getGlobalState(s, s2, s3);

                    double max = -10;
                    foreach (State ns1 in s.getNextList()) {
                        nextutil = 10000000;
                        //search through adjacency lists of p2 and p3
                        foreach (State ns2 in s2.getNextList())
                        {
                            GlobalState ngs;
                            double temputil = 0;//temp util for given ns2 state

                            foreach (State ns3 in s3.getNextList())
                            {
                                if (getGlobalState(ns1, ns2, ns3) == null || !gs.getNextGstate().Contains(getGlobalState(ns1, ns2, ns3)))
                                    continue;
                                ngs = getGlobalState(ns1, ns2, ns3);
                             //   temputil += ngs.getP1util() * ns3.getProb();
                                temputil += (ngs.getP1util() + ngs.getutilite(ite-1)) * ns3.getProb();
                                //      System.Diagnostics.Debug.WriteLine("nextUtil =" + nextutil + " " + ns.getId() + " " + ns2.getId() + " " + ns3.getId() + " " + ns2.getProb() + " " + ns3.getProb());
                            }

                            if (temputil < nextutil)
                            {
                                nextutil = temputil;
                                System.Diagnostics.Debug.WriteLine("min updated " + nextutil + " " + s.getId() + " " + s2.getId() + " " + s3.getId()+" "+ns2.getId());
                            }

                        }
                        currentutil += nextutil * s2.getProb() * s3.getProb();
                        if (max < nextutil)
                        {
                            max = nextutil;
                            System.Diagnostics.Debug.WriteLine("max updated " + max + " " + s.getId() + " " + s2.getId() + " " + s3.getId() + " " + ns1.getId());
                        }
                        gs.setutilite(ite, max);
                    
                    }
                    
                    
             //       System.Diagnostics.Debug.WriteLine("CurrentUtil =" + currentutil + " " + s.getId() + " " + s2.getId() + " " + s3.getId() + " " + s2.getProb() + " " + s3.getProb());
                }
            }
            return currentutil;
        }

        private static double calchonExpStatePayoff(State s, List<State> list1, List<State> list2,int ite)
        {
            double nextutil = 0;
            double currentutil = 0;

           
            //determines all the possible current global states given the current local state s 
            foreach (State s2 in list1)
            {
                foreach (State s3 in list2)
                {
                    //(s,s2,s3) is one such possible global state

                    if (getGlobalState(s, s2, s3) == null)
                        continue;
                    GlobalState gs = getGlobalState(s, s2, s3);



                    nextutil = 10000000;
                    //search through adjacency lists of p2 and p3
                    foreach (State ns2 in s2.getNextList())
                    {
                        GlobalState ngs;
                        double temputil = 0;//temp util for given ns2 state

                        foreach (State ns1 in s.getNextList()) {

                            foreach (State ns3 in s3.getNextList())
                            {//going through state pairs of honest players p1 and p3
                               // GlobalState nextstate = getGlobalState(ns1, ns2, ns3);
                                if (getGlobalState(ns1, ns2, ns3) == null || !gs.getNextGstate().Contains(getGlobalState(ns1, ns2, ns3)))
                                    continue;
                                ngs = getGlobalState(ns1, ns2, ns3);
                            //    temputil += (ngs.getP1util() + ns1.getExputil(ite)) * ns3.getProb() * ns1.getProb();
                              //  gs.setutilite(ite + 1, gs.getutilite(ite + 1) + (ngs.getP1util() + ngs.getutilite(ite)) * ns3.getProb() * ns1.getProb());
                                temputil += (ngs.getP1util() + ngs.getutilite(ite-1)) * ns3.getProb() * ns1.getProb();
                                //      System.Diagnostics.Debug.WriteLine("nextUtil =" + nextutil + " " + ns.getId() + " " + ns2.getId() + " " + ns3.getId() + " " + ns2.getProb() + " " + ns3.getProb());
                            }
                        
                        }

                        

                        if (temputil < nextutil)
                        {
                            nextutil = temputil;
                        }

                    }
                    currentutil += nextutil * s2.getProb() * s3.getProb();
                    gs.setutilite(ite, nextutil);
                    //       System.Diagnostics.Debug.WriteLine("CurrentUtil =" + currentutil + " " + s.getId() + " " + s2.getId() + " " + s3.getId() + " " + s2.getProb() + " " + s3.getProb());
                }
            }
            return currentutil;
        }

        private static GlobalState getGlobalState(State s, State s2, State s3)
        {
            foreach (GlobalState gs in gslist)
            {
                if (gs.getS1().Equals(s) && gs.getS2().Equals(s2) && gs.getS3().Equals(s3))
                {
                    return gs;
                }
            }
            return null;
        }

        private static void calculateUtility(GlobalState gstate, int id1, int id2, int id3)
        {
            bool p1win, p2win, p3win, p12win, p23win, p13win, nowin;
            p1win = (id2 == id3) && ((id1 > id2 && id1 - id2 == 1) || id2 - id1 == 2);
            p2win = (id3 == id1) && ((id2 > id3 && id2 - id3 == 1) || id3 - id2 == 2);
            p3win = (id1 == id2) && ((id3 > id1 && id3 - id1 == 1) || id1 - id3 == 2);
            p12win = (id1 == id2) && ((id3 < id1 && id1 - id3 == 1) || id3 - id1 == 2);
            p23win = (id2 == id3) && ((id1 < id2 && id2 - id1 == 1) || id1 - id2 == 2);
            p13win = (id3 == id1) && ((id2 < id3 && id3 - id2 == 1) || id2 - id3 == 2);

            nowin = ((id1 == id2) && (id1 == id3)) || ((id1 + id2 + id3 == 3) && (id1 * id2 * id3 == 0));
            // if(nowin)
            //     System.Diagnostics.Debug.WriteLine(id1 + " " + id2 + " " + id3 + " nowin");

            if (p1win)
            {
                gstate.setUtility(2 , -1 , -1 );
            }
            else if (p2win)
            {
                gstate.setUtility(-1 , 2 , -1 );
            }
            else if (p3win)
            {
                gstate.setUtility(-1 , -1 , 2 );
            }
            else if (p12win)
            {
                gstate.setUtility(1, 1 , -2 );
            }
            else if (p23win)
            {
                gstate.setUtility(-2 , 1 , 1 );
            }
            else if (p13win)
            {
                gstate.setUtility(1 , -2 , 1 );
            }
            else if (nowin)
            {
                gstate.setUtility(0 , 0 , 0 );
            }
        }


    }

    class State
    {
        private int id;
        private List<State> nextList;
        private double prob;
        private double exputil;
        private int ite;
        private double[] utilite;
        public State(int id,int iters)
        {
            this.id = id;
            this.nextList = new List<State>();
            this.prob = 1;
            this.exputil = 0;   
            this.utilite = new double[iters+2];
        }


       
        public void setExputil(double exputil)
        {
            this.exputil = exputil;

        }

        public double getExputil()
        {
            return this.exputil;
        }

        public double getExputil(int iter)
        {

            return this.utilite[iter];
        }

        public void setExputil(int iter,double value)
        {

            this.utilite[iter]=value;
        }
        public List<State> getNextList()
        {

            return this.nextList;
        }
        public void addToNextList(State s)
        {

            nextList.Add(s);
        }
        public int getId()
        {

            return this.id;
        }
        public void setProb(double prob)
        {

            this.prob = prob;
        }

        public double getProb()
        {

            return this.prob;
        }
    }
    class GlobalState
    {
        private State s1, s2, s3;
        private List<GlobalState> nextgstateList;
        private double p1_utility;
        private double p2_utility;
        private double p3_utility;
        private double p1_minutility;
        private double p2_minutility;
        private double p3_minutility;
        private double[] utilite;
        public GlobalState(State s1, State s2, State s3,int iter)
        {
            this.s1 = s1;
            this.s2 = s2;
            this.s3 = s3;
            this.nextgstateList = new List<GlobalState>();
            this.p1_utility = 0;
            this.p2_utility = 0;
            this.p3_utility = 0;
            this.p1_minutility = 10;
            this.p2_minutility = 10;
            this.p3_minutility = 10;
            this.utilite=new double[iter+2];
         /*   for (int i = 0; i < utilite.Count(); i++)
            {
                utilite[i] = -100;
            }*/
        }

        public State getS1()
        {
            return s1;
        }

        public State getS2()
        {
            return s2;
        }

        public State getS3()
        {
            return s3;
        }
        public double getP1util()
        {
            return this.p1_utility;
        }
        public List<GlobalState> getNextGstate()
        {
            return this.nextgstateList;

        }
        public void setUtility(double p1_utility, double p2_utility, double p3_utility)
        {

            this.p1_utility = p1_utility;
            this.p2_utility = p2_utility;
            this.p3_utility = p3_utility;
        }
        public void setutilite(int iter,double value){

            this.utilite[iter]=value;
        }

        public double getutilite(int iter)
        {

           return this.utilite[iter];
        }

    }
    class Player
    {
        private List<State> stateList;
        private State init;
        private bool honest;
        private double[] prob_dist;
        public Player(List<State> stateList, State init, bool honest)
        {
            this.stateList = stateList;
            this.init = init;
            this.honest = honest;

        }
        public void localGraph(List<State> states)
        {
            for (int i = 0; i < states.Count; i++)
            {


            }
        }
        public void setProb_dist(double[] prob_dist)
        {
            this.prob_dist = prob_dist;

        }

        public double[] getProb_dist()
        {
            return this.prob_dist;

        }

        public List<State> getStateList()
        {

            return this.stateList;
        }
        public State getInit()
        {

            return this.init;
        }
        public bool isHonest()
        {

            return this.honest;
        }
    }
}
