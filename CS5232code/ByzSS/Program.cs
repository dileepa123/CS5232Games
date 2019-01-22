using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByzSS
{

    class Program
    {
        static List<GlobalState> gstates = new List<GlobalState>();

        static void Main(string[] args)
        {
            double alpha = 0.7;

            Player p1 = new Player(new State("0", 1), true, new List<double>[] { 
                new List<double>{alpha,1-alpha},
                new List<double>{0.5,0.5},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1} // init local prob distributions
            }, alpha);
            Player p2 = new Player(new State("0", 1), true, new List<double>[] { 
                new List<double>{alpha,1-alpha},
                new List<double>{0.5,0.5},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1}
            }, alpha
                 );
            Player p3 = new Player(new State("0", 1), false, new List<double>[] {      
                new List<double>{alpha,1-alpha},
                new List<double>{0.5,0.5},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1}
            }, alpha
                );

            Player ph = new Player(new State("0", 1), false, new List<double>[] {      
                new List<double>{alpha,1-alpha},
                new List<double>{0.5,0.5},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1}
            }, alpha
                );
            p1.createGraph();
            p2.createGraph();
            p3.createGraph();
            ph.createGraph();


            globalGraph(p1.getStageList(), p2.getStageList(), p3.getStageList());
            calculateValues(p1.getStageList(), p2.getStageList(), p3.getStageList(), false);//calculate utility values for the given horizon
            double v = gstates[0].getValues()[gstates[0].getValues().Count() - 1];


            globalGraph(ph.getStageList(), p2.getStageList(), p3.getStageList());
            calculateValues(ph.getStageList(), p2.getStageList(), p3.getStageList(), true);//ph is honest
            double u = gstates[0].getValues()[gstates[0].getValues().Count() - 1];

            if (u >= v)
            {

                System.Diagnostics.Debug.WriteLine("Nash Equilibrium");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Not a Nash Equilibrium");
            }

        }

        private static void calculateValues(List<State>[] l1, List<State>[] l2, List<State>[] l3, bool honest)
        {
            //here, number of stages is equal for all the players
            List<State> p1states;
            List<State> p2states;
            List<State> p3states;
            System.Diagnostics.Debug.WriteLine(gstates.Count);
            for (int i = 1; i < State.getHorizon(); i++)
            {
                for (int stage = 0; stage < l1.Count(); stage++)
                {

                    p1states = l1[stage];
                    p2states = l2[stage];
                    p3states = l3[stage];
                    foreach (State s1 in p1states)
                    {

                        double utility = 0;
                        double max = 0;
                        double min = 100000;
                        GlobalState gs=null;
                        foreach (State ns1 in s1.getNextlist())
                        {//for each possibility of player1
                            utility = 0;
                            int count = 0;
                            foreach (State s2 in p2states)
                            {
                                min = 1000000;
                                foreach (State ns2 in s2.getNextlist()) {
                                    foreach (State s3 in p3states)//for each current statepair of other players
                                    {

                                        gs = getGlobalState(s1, s2, s3);
                                        double value = 0;
                                        if (!honest)
                                            value = calculateLocalValue(gs, i, ns1,ns2);//probability weighted value for given state pair
                                        else
                                        {
                                            value = calculateHonestLocalValue(gs, i,ns2);//no need of specific next state
                                        }
                                        utility += value;
                                        
                                    }
                                    if (utility < min)
                                    {
                                        min = utility;
                                    }
                                    gs.setValues(min, i);
                                }
                                

                            }
                            count++;
                            if (count == 1 && honest)//only one iteration for honest player
                            {
                                max = min;
                                break;
                            }

                            if (min> max)
                                max = min;
                        }
                        s1.setValues(max, i);


                    }

                }



            }
        }

        private static double calculateLocalValue(GlobalState gs, int horizon, State p1next,State p2next)
        {
            State s1 = gs.gets1();
            State s2 = gs.gets2();
            State s3 = gs.gets3();
            double localValue = 0;
            double max = 0;
            localValue = 0;
            

                foreach (State ns3 in s3.getNextlist())
                {
                    if (horizon == 1)
                    {
                        localValue +=  ns3.getProbability() * getGlobalState(p1next, p2next, ns3).getPayoff();
                    }
                    else
                    {
                        localValue += ns3.getProbability() * getGlobalState(p1next, p2next, ns3).getValues()[horizon - 1];
                    }


                }
            



            return localValue;
        }

        private static double calculateHonestLocalValue(GlobalState gs, int horizon,State p2next)
        {
            State s1 = gs.gets1();
            State s2 = gs.gets2();
            State s3 = gs.gets3();
            double localValue = 0;

            localValue = 0;
            foreach (State ns1 in s1.getNextlist())
            {
                

                    foreach (State ns3 in s3.getNextlist())
                    {
                        if (horizon == 1)
                        {
                            localValue += ns1.getProbability()  * ns3.getProbability() * getGlobalState(ns1, p2next, ns3).getPayoff();
                        }
                        else
                        {
                            localValue += ns1.getProbability()  * ns3.getProbability() * getGlobalState(ns1, p2next, ns3).getValues()[horizon - 1];
                        }


                    }
                


            }




            return localValue;
        }

        private static GlobalState getGlobalState(State s1, State s2, State s3)
        {
            // System.Diagnostics.Debug.WriteLine(gstates.Count);
            foreach (GlobalState gs in gstates)
            {

                if (gs.gets1().getId().Equals(s1.getId()) && gs.gets2().getId().Equals(s2.getId()) && gs.gets3().getId().Equals(s3.getId()))
                {
                    // System.Diagnostics.Debug.WriteLine(gs.gets1().getId() + " " + gs.gets2().getId() + " " + gs.gets3().getId());
                    return gs;
                }
            }
            return null;
        }
        static void globalGraph(List<State>[] l1, List<State>[] l2, List<State>[] l3)
        {
            //here, number of stages is equal for all the players
            List<State> p1states;
            List<State> p2states;
            List<State> p3states;
            for (int i = 0; i < l1.Count(); i++)
            {
                p1states = l1[i];
                p2states = l2[i];
                p3states = l3[i];
                foreach (State s1 in p1states)
                {
                    System.Diagnostics.Debug.WriteLine("s1 is" + s1.getId());
                    foreach (State s2 in p2states)
                    {
                        foreach (State s3 in p3states)
                        {
                            GlobalState gs = new GlobalState(s1, s2, s3, 0);
                            gstates.Add(gs);


                            if (!(s1.getId().Length < l1.Count()))
                            {//reached a terminal state   
                                calculatePayoff(gs);
                            }

                        }
                    }
                }
            }
        }


        private static void calculatePayoff(GlobalState gs)
        {
            int utility = 0;
            int everyone = 1;
            int noone = 0;
            int onlyme = 4;
            int meandother = 3;
            int onlyother =-2;
            int twoothers = -1;

            String str1 = gs.gets1().getId();
            String str2 = gs.gets2().getId();
            String str3 = gs.gets3().getId();

            int c1 = (int)(str1.ToCharArray()[1]) - 64;
            int c2 = (int)(str2.ToCharArray()[1]) - 64;
            int c3 = (int)(str3.ToCharArray()[1]) - 64;
            int p1bc=(int)(str2.ToCharArray()[3]) - 64;
            int p3bc=(int)(str2.ToCharArray()[4]) - 64;
            int m1send = (int)(str1.ToCharArray()[str1.Length - 2]) - 64;
            int m2send = (int)(str1.ToCharArray()[str1.Length - 1]) - 64;
            int msend = m1send * m2send;

            int m1sendp2 = (int)(str2.ToCharArray()[str1.Length - 2]) - 64;
            int m2sendp2 = (int)(str2.ToCharArray()[str1.Length - 1]) - 64;
            int msendp2 = m1sendp2 * m2sendp2;

            int p = c1 ^ c2 ^ c3;
            int p1,p2,p3;
            p1=p^(p1bc);
            p3=p^(p3bc);

            int msendp3=0;// p3 is honest so he sends or not sends both messages
            

            if (p3 == 1 && c3 == 1)
            {
                msendp3 = 1;
            }
            String state1id = "";
            String state2id = "";
            String state3id = "";
          
                if (m1send == 0 && m2send == 0)//send to no one
                {
                    
                    if(m1sendp2==1&&msendp3==1){
                        
                        
                        utility = onlyme;
                    }
                    else{
                        utility = noone;
                    }
                    
                    
                }
                else if (msend==0)//send only to p2 or p3
                {
                    state1id = "cheat";
                    if ((m1sendp2 == 1 && msendp3==1 &&m1send==1)||(msendp2 == 1 && msendp3==1 &&m2send==1))
                    {//p1 and p2                                  //p1 and p3
                        utility = meandother;
                    }
                    else if ((m1sendp2 == 0 && msendp3 == 1 && m1send == 1)||(m2send==1&&m2sendp2==1&&m1sendp2==0))
                    {//p2                                                      //p3
                        utility = onlyother;
                    }
                    else
                    {
                        utility = noone; 
                    }
                    
                }
                else//msend=1 send to both
                {
                   
                    if (msendp3 == 1 && msendp2 == 1)//all send to all
                    {
                        utility = everyone;
                    }
                    else if (msendp3 == 1 && m1sendp2 == 1)//p3 sends but p2 sends only for p1
                    {//p1 and p2
                        utility = meandother;
                    }
                    else if (msendp3 == 1 && m2sendp2 == 1) {//p2 sends only to p3
                        utility = twoothers;//p3 and p2
                    }
                    else if (msendp3 == 1) {//p2 sends to no one
                        utility = onlyother;
                    }
                    else if (msendp3 == 0&&m2sendp2==1) {
                        utility = onlyother;
                    }
                    else
                    {
                        utility = noone;
                    }
                }

                state1id = calculateState(p1,c1,m1send,m2send);
                state2id = calculateState(p, c2, m1sendp2,m2sendp2);

           
            gs.setPayoff(utility);
            State s1 = gs.gets1();
            State s2 = gs.gets2();
            State s3 = gs.gets3();
            if (utility==everyone||!(state1id.Equals("")&&state2id.Equals("")))//cheat or end the protocol add new final state
            {

                State news1 = new State(state1id, s1.getProbability());
                s1.getNextlist().Add(news1);
                State news2 = new State(state2id, s2.getProbability());
                s2.getNextlist().Add(news2);
                State news3 = new State(state3id, s3.getProbability());
                s3.getNextlist().Add(news3);
                gstates.Add(new GlobalState(news1, news2, news3, 0));//new state has utility 0

                //loop around new state
                news1.getNextlist().Add(news1);
                news2.getNextlist().Add(news2);
                news3.getNextlist().Add(news3);

            }
            else
            {// if protocol restarts
                s1.getNextlist().Add(gstates[0].gets1());
                s2.getNextlist().Add(gstates[0].gets2());
                s3.getNextlist().Add(gstates[0].gets1());
            }

        }

        private static string calculateState(int p, int c, int m1sendp,int m2sendp)
        {
            String result = "";
            int msend=m1sendp*m2sendp;
            if (p == 1 && c == 1 && msend == 0) {//supposed to send not sent
                result = "cheat";
            }
            else if (!(p == 1 && c == 1)&&(m1sendp + m2sendp > 0)) {//supposed to not send but sent

                result = "cheat";
            }
            return result;
        }
    }

    public class GlobalState
    {
        private State s1, s2, s3;
        private double payoff = 0;
        private List<GlobalState> nextlist;
        private static int horizon = 7;
        private double[] values;

        public GlobalState(State s1, State s2, State s3, double payoff)
        {

            this.s1 = s1;
            this.s2 = s2;
            this.s3 = s3;
            this.payoff = payoff;
            this.nextlist = new List<GlobalState>();
            this.values = new double[horizon];
        }
        public State gets1()
        {
            return this.s1;

        }
        public State gets2()
        {
            return this.s2;

        }

        public State gets3()
        {
            return this.s3;

        }

        public double getPayoff()
        {

            return this.payoff;
        }
        public void setPayoff(double payoff)
        {
            this.payoff = payoff;

        }

        public double[] getValues()
        {
            return this.values;
        }

        public void setValues(double utility, int index)
        {
            this.values[index] = utility;

        }

    }
    public class State
    {

        private String id;
        private List<State> nextlist;
        private double probability;
        private static int horizon = 2;
        private double[] values;
        public State(String id, double probability)
        {

            this.id = id;
            this.nextlist = new List<State>();
            this.probability = probability;
            this.values = new double[horizon];

        }

        public double getProbability()
        {
            return this.probability;
        }
        public String getId()
        {
            return this.id;
        }
        public List<State> getNextlist()
        {
            return this.nextlist;
        }
        public double[] getValues()
        {
            return this.values;
        }

        public void setValues(double utility, int index)
        {
            this.values[index] = utility;

        }
        public static int getHorizon()
        {

            return horizon;
        }
    }

    public class Player
    {
        private State init;
        private double alpha;
        private bool isByzantine;
        private int[] stages;
        private List<State>[] stagelist;
        private List<double>[] prob_dist;//probability distributions for states
        public Player(State s, bool isByzantine, List<double>[] prob_dist, double alpha)
        {
            this.init = s;
            this.isByzantine = isByzantine;
            if (isByzantine)
            {
                stages = new int[] { 1, 2, 2, 2, 2, 2, 2 };
            }
            else
            {
                stages = new int[] { 1, 2, 2, 1, 1, 1, 1 };
            }
            stagelist = new List<State>[stages.Count()];
            if (!isByzantine)
                this.prob_dist = prob_dist;
            else this.prob_dist = new List<double>[] {      
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1},
                new List<double>{1}
            };
            this.alpha = alpha;

            //first 3 stages are probabilistic
        }

        public bool getIsByzantine()
        {
            return isByzantine;
        }
        public State getinit()
        { // gets the player's initial state, at the end this refers to his local graph
            return this.init;

        }

        public int[] getStages()
        {// get the action counts in each stage

            return this.stages;
        }
        public List<State>[] getStageList()
        {// get state list for each stage

            return this.stagelist;
        }

        public List<double>[] getProb_dist()
        {//get probability distributions for each stage

            return this.prob_dist;
        }
        public void createGraph()
        {
            int stage = 1;

            List<State> curlist = new List<State>();
            List<State> fulist = new List<State>();
            curlist.Add(init);
            stagelist[0] = curlist;
            while (stage < stages.Count())
            { //start from stage 1
                foreach (State s in curlist)
                { //current list is the list of states in the current stage
                    for (int i = 0; i < stages[stage]; i++)
                    {
                        State Nstate;
                        if (s.getProbability() * this.prob_dist[stage - 1].Count != 1)
                            Nstate = new State(s.getId() + (i).ToString(), s.getProbability() * this.prob_dist[stage - 1].ElementAt(i));//create a new state for the next stage
                        else Nstate = new State(s.getId() + (i).ToString(), s.getProbability());
                        s.getNextlist().Add(Nstate);// add a pointer from the current state
                        fulist.Add(Nstate);//add the state to the future stage
                        System.Diagnostics.Debug.Write(Nstate.getId() + " ");
                    }

                }
                System.Diagnostics.Debug.WriteLine("");
                stagelist[stage] = fulist;
                stage++;
                curlist = fulist;

                fulist = new List<State>();
            }

        }

    }
}
