using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppleGame
{
    class Program
    {
        const int n = 3;//number of players
       
   
		static bool _debug = false;

        //player configs
        static bool[] bybool = new bool[] { false, false, false };//2nd player is byzantine
        static List<int> byZ = new List<int>();//byzantine list
        static List<int> Al = new List<int>();//altruistic list

        //state configs
        static List<State>[] locstatelist = new List<State>[n];
        static List<GlobalState> gstates = new List<GlobalState>();// define set of global states as an array

        
        
        //action configs
        static List<GlobalAction> byzAc = new List<GlobalAction>();
        static List<GlobalAction> AlAc = new List<GlobalAction>();
        static List<GlobalAction> AlrAc = new List<GlobalAction>();
        static List<GlobalAction> AlmrAc = new List<GlobalAction>();//multirational actions
        static List<GlobalAction> multiAc = new List<GlobalAction>();
        static List<int>[] localactionset = new List<int>[n];

        static List<double>[] vcurrent = new List<double>[n];//each i has a list(states) of double arrays
        static List<double>[] vprev = new List<double>[n];//each i has a list(states) of double arrays

        static List<double>[] ucurrent = new List<double>[n];//each i has a list(states) of double arrays
        static List<double>[] uprev = new List<double>[n];//each i has a list(states) of double arrays
        static int[] Actions = new int[n];

        static double[] ratValue = new double[n];

        static double alpha = 0.2;
        static double eps = 1.1, delta = 0.1, beta = 0.5;
        static void Main(string[] args)
        {
			if (_debug) {
				Console.WriteLine ("App start");
			}
            //update players
            for (int i = 0; i < n; i++)
            {
                if (bybool[i])
                    byZ.Add(i);
                else
                    Al.Add(i);
            }// update byzantine and altruistic lists


            //update states

            //spec
            for (int i = 0; i < n; i++)
            {//for each player
                List<State> slist = new List<State>();//state list(set) for player i

                AState.AddStates(slist,i);
                   
                locstatelist[i] = slist;// local state list for player i
            }// construct local state list
            
            /*construct global states*/
            List<State> before = new List<State>();
            computeGstates(before, 0, gstates, locstatelist);// determine the list of global states
            int nofstates = gstates.Count;
			Console.WriteLine("state count =" + nofstates);
            //update actions
            for (int i = 0; i < n; i++)
            {
                localactionset[i] = new List<int>();
                for (int j = 0; j <= n; j++)
                {
                    (localactionset[i]).Add(j);//0=share, j!=0 implies ai=shootj
                }
                
            }
            List<int> abefore = new List<int>();

        //    if(byZ.Count>0)
                computegActions(abefore, 0, byzAc, byZ.Count);
            abefore = new List<int>();
            
            computegActions(abefore, 0, AlAc, Al.Count);//action list depends on the size of the set
            abefore = new List<int>();
            computegActions(abefore, 0, AlrAc, Al.Count - 1);//action list when player is rational
            abefore = new List<int>();
            computegActions(abefore, 0, multiAc, 2);//create actions pairs
            abefore = new List<int>();
            if (Al.Count > 2)
                computegActions(abefore, 0, AlmrAc, Al.Count - 2);//Al when two players are rational

            //actions updated

            

            
  //          String result1 = Nashepdelta(vcurrent, vprev, ucurrent, uprev);
  //          Console.WriteLine("first part done");
            String result2 = Nashepdeltamul(vcurrent, vprev, ucurrent, uprev, 0, 2);     
            Console.WriteLine(result2);
          System.Diagnostics.Debug.WriteLine("mul result " + result2);
           





        }

       

        private static String Nashepdelta(List<double>[] vcurrent, List<double>[] vprev, List<double>[] ucurrent, List<double>[] uprev)
        {
            String result = "";

            
            
            //algo


            double M = 4;
            //beta^k < (1-beta)*delta/5M
            //k>log((1-beta)*delta/5M)/log(beta)
            double k = Math.Ceiling(Math.Log(((1 - beta) * delta) / (24 * M)) / Math.Log(beta));
            double EiK = (6 * M * Math.Pow(beta, k)) / (1 - beta);

            System.Diagnostics.Debug.WriteLine("k =" + k);

            double[] bigDelta = new double[n];
            double ep1, ep2;
            int count = 0;
            foreach (int i in Al)
            {//for each altruistic player

                init(i,"vc");
                init(i,"vp");
                for (int t = 0; t < k; t++)
                {
                
                    
                    
                    bybool[i] = true;//make i byzantine            

                    reward_calc(vprev, vcurrent, i, bybool[i]);
                    vprev[i] = vcurrent[i];//values calculated for all s

                   
                    //  System.Diagnostics.Debug.WriteLine(ucurrent[i][getGlobalStateIndex(gstates[0].getLocalStates())] + " i " + i + " t " + t);
                

                }

                ratValue[i] = vcurrent[i][getGlobalStateIndex(gstates[0].getLocalStates())];
                vcurrent[i] = null;
                vprev[i] = null;
                GC.Collect();
                init(i,"uc");
                init(i,"up");
                for (int t = 0; t < k; t++)
                {
                    bybool[i] = false;//remove byzantine role from i
                    reward_calc(uprev, ucurrent, i, bybool[i]);
                    uprev[i] = ucurrent[i];
                }
                bigDelta[i] = ratValue[i] - ucurrent[i][getGlobalStateIndex(gstates[0].getLocalStates())];
                ucurrent[i] = null;
                uprev[i] = null;
                GC.Collect();
            }
            

            foreach (int i in Al)
            {
               // ratValue[i] = vcurrent[i][getGlobalStateIndex(gstates[0].getLocalStates())];
               // bigDelta[i] = vcurrent[i][getGlobalStateIndex(gstates[0].getLocalStates())] - ucurrent[i][getGlobalStateIndex(gstates[0].getLocalStates())];//vi(s_0)
                //  System.Diagnostics.Debug.WriteLine(vcurrent[i][getGlobalStateIndex(gstates[0].getLocalStates())]);
                //  System.Diagnostics.Debug.WriteLine(ucurrent[i][getGlobalStateIndex(gstates[0].getLocalStates())]);
                ep1 = bigDelta[i] - 2 * EiK;
                ep2 = bigDelta[i] + 2 * EiK;
                if (eps < ep1)
                {
                    result = "FAIL";
                    break;
                }
                else if (ep2 < eps)
                {
                    count++;
                }

            }
            if (count == Al.Count)// if fail count does not reach Al count
            {
                result = "NEeps";
            }
            else if (!result.Equals("FAIL"))
            {
                result = "NEepsdelta";
            }
            // System.Diagnostics.Debug.WriteLine(result);
            return result;
        }

        private static void reward_calc(List<double>[] prev,List<double>[] current,int i,bool biz){

            
           /* Parallel.ForEach(gstates, (s) =>
            {*/
			Parallel.For(0, gstates.Count,index =>
            {
				if (_debug) {
					Console.WriteLine(System.Threading.Thread.CurrentThread.ManagedThreadId);
				}
                GlobalState s = gstates[(index * 128)%gstates.Count];
				//GlobalState s = gstates[(index * 128)%59049];
                double max = 0;
                double exp = 0;
                foreach (int a_i in localactionset[i])//for each action of rational i
                {
                    double minbyz = 20;
                    if (byzAc.Count == 0)// if no byzantines
                    {
                        double reward = 0;
                        // double ureward = 0;
                        foreach (GlobalAction a_al in AlrAc)//for each altruistic action Al-i
                        {
                            setglobalAction(i, a_i, a_al);
                            if (BT(s, Actions, gstates) != null)
                            {
                                GlobalState snext = BT(s, Actions, gstates);
                                reward += GlobalPr(s, Actions) * (H(i, s, Actions) + beta * prev[i][getGlobalStateIndex(snext.getLocalStates())]);
                                //    ureward += GlobalPr(s, Actions) * (H(i, s, Actions) + beta * uprev[i][getGlobalStateIndex(snext.getLocalStates())]);

                            }
                        }//expected reward calculated for a_z,a_i
                        minbyz = reward;
                    }
                    else {
                        foreach (GlobalAction a_z in byzAc)//for each byz action Z
                        {
                            double reward = 0;
                            // double ureward = 0;
                            foreach (GlobalAction a_al in AlrAc)//for each altruistic action Al-i
                            {
                                setglobalAction(i, a_i, a_z, a_al);
                                if (BT(s, Actions, gstates) != null)
                                {
                                    GlobalState snext = BT(s, Actions, gstates);
                                    reward += GlobalPr(s, Actions) * (H(i, s, Actions) + beta * prev[i][getGlobalStateIndex(snext.getLocalStates())]);
                                    //    ureward += GlobalPr(s, Actions) * (H(i, s, Actions) + beta * uprev[i][getGlobalStateIndex(snext.getLocalStates())]);

                                }
                            }//expected reward calculated for a_z,a_i
                            if (reward < minbyz)
                                minbyz = reward;
                            //     System.Diagnostics.Debug.WriteLine("reward =" + minbyz);
                        }//minbyz calculated
                    
                    }
                   
  
                    if (minbyz > max)
                        max = minbyz;
                    exp += Pr(i, s, Actions) * minbyz;
                }//maxrat calculated
                if (biz)
                    current[i][getGlobalStateIndex(s.getLocalStates())] = max;//v^k_i(s)=maxminE
                else
                    current[i][getGlobalStateIndex(s.getLocalStates())] = exp;//v^k_i(s)=maxminE
            });

          }
        private static String Nashepdeltamul(List<double>[] vcurrent, List<double>[] vprev, List<double>[] ucurrent, List<double>[] uprev, int i0, int j0)
        {
            String result = "";
            double min_distance = 20, deltastar = 0;
            List<int> ratlist = new List<int>();
            ratlist.Add(i0);
            ratlist.Add(j0);

            while (result.Equals("undecided") || result.Equals(""))
            {
                //initialize vi and ui
               for (int i = 0; i < n; i++)
                {
                    vcurrent[i] = new List<double>();
                    vprev[i] = new List<double>();
                    ucurrent[i] = new List<double>();
                    uprev[i] = new List<double>();
                    for (int s = 0; s < gstates.Count; s++)
                    {
                        vcurrent[i].Add(0);
                        vprev[i].Add(0);
                        ucurrent[i].Add(0);
                        uprev[i].Add(0);
                    }
                }
               
                //algo


                double M = 4;
                //beta^k < (1-beta)*delta/5M
                //k>log((1-beta)*delta/5M)/log(beta)
                double k = Math.Ceiling(Math.Log(((1 - beta) * delta) / (24 * M)) / Math.Log(beta));
                double EiK = (6 * M * Math.Pow(beta, k)) / (1 - beta);

                System.Diagnostics.Debug.WriteLine("k =" + k);
                Parallel.For(0, gstates.Count, index =>
            //    Parallel.For(0, 729, index =>
             {
                 GlobalState s = gstates[(index * 128) % gstates.Count];
               //  GlobalState s = gstates[(index * 128) % 729];
                 for (int t = 0; t < k; t++)
                 {
                     //    foreach (int i in ratlist)//i,j rationals
                     //    {//for each altruistic player, player of interest is rational
                     bybool[i0] = true;//make i byzantine
                     bybool[j0] = true;//make j byzantine

                     double max = 0;
                     
                     foreach (GlobalAction a_ij in multiAc)//for each action of rational ij 2
                     {
                         double minbyzi = 20;
                         double minbyzj = 20;
                         foreach (GlobalAction a_z in byzAc)//for each byz action Z
                         {
                             double rewardi = 0;
                             double rewardj = 0;
                             foreach (GlobalAction a_al in AlmrAc)//for each altruistic action,two rat players Al-2
                             {
                                 setglobalAction(i0, j0, a_ij, a_z, a_al);//call overridden method
                                 if (BT(s, Actions, gstates) != null)
                                 {
                                     GlobalState snext = BT(s, Actions, gstates);
                                     rewardi += GlobalPr(s, Actions) * (H(i0, s, Actions) + beta * vprev[i0][getGlobalStateIndex(snext.getLocalStates())]);
                                     rewardj += GlobalPr(s, Actions) * (H(j0, s, Actions) + beta * vprev[j0][getGlobalStateIndex(snext.getLocalStates())]);

                                 }
                             }//expected reward calculated for a_z,a_ij
                             if (AlmrAc.Count == 0)
                             {
                                 setglobalAction(i0, j0, a_ij, a_z, null);//call overridden method
                                 if (BT(s, Actions, gstates) != null)
                                 {
                                     GlobalState snext = BT(s, Actions, gstates);
                                     rewardi = (H(i0, s, Actions) + beta * vprev[i0][getGlobalStateIndex(snext.getLocalStates())]);
                                     rewardj = (H(j0, s, Actions) + beta * vprev[j0][getGlobalStateIndex(snext.getLocalStates())]);

                                 }
                             }
                             if (rewardi < minbyzi)
                                 minbyzi = rewardi;
                             if (rewardj < minbyzj)
                                 minbyzj = rewardj;
                             //min reward calculated for each a_ij
                             //     System.Diagnostics.Debug.WriteLine("reward =" + minbyz);
                         }//minbyz calculated
                         if (byZ.Count == 0)
                         {
                              double rewardi = 0;
                                 double rewardj = 0;
                                 foreach (GlobalAction a_al in AlmrAc)//for each altruistic action,two rat players Al-2
                                 {
                                     setglobalAction(i0, j0, a_ij, null, a_al);//call overridden method
                                     if (BT(s, Actions, gstates) != null)
                                     {
                                         GlobalState snext = BT(s, Actions, gstates);
                                         rewardi += GlobalPr(s, Actions) * (H(i0, s, Actions) + beta * vprev[i0][getGlobalStateIndex(snext.getLocalStates())]);
                                         rewardj += GlobalPr(s, Actions) * (H(j0, s, Actions) + beta * vprev[j0][getGlobalStateIndex(snext.getLocalStates())]);

                                     }
                                 }//expected reward calculated for a_z,a_ij
                                 if (AlmrAc.Count == 0)
                                 {
                                     setglobalAction(i0, j0, a_ij, null, null);//call overridden method
                                     if (BT(s, Actions, gstates) != null)
                                     {
                                         GlobalState snext = BT(s, Actions, gstates);
                                         rewardi = (H(i0, s, Actions) + beta * vprev[i0][getGlobalStateIndex(snext.getLocalStates())]);
                                         rewardj = (H(j0, s, Actions) + beta * vprev[j0][getGlobalStateIndex(snext.getLocalStates())]);

                                     }
                                 }
                                 
                                     minbyzi = rewardi;
                                     minbyzj = rewardj;
                                 //min reward calculated for each a_ij
                                 //     System.Diagnostics.Debug.WriteLine("reward =" + minbyz);
                             

                         }
                         /* if (minbyz > max)
                              max = minbyz;
                          exp += Pr(i, s, Actions) * minbyz;*/
                         //min reward is used to maximize a different reward function
                         if (alpha * minbyzi + (1 - alpha) * minbyzj > max)
                         {
                             max = alpha * minbyzi + (1 - alpha) * minbyzj;
                             vcurrent[i0][getGlobalStateIndex(s.getLocalStates())] = minbyzi;
                             vcurrent[j0][getGlobalStateIndex(s.getLocalStates())] = minbyzj;
                         }

                     }//maxrat calculated
                     // vcurrent[i0][getGlobalStateIndex(s.getLocalStates())] = max;//v^k_i(s)=maxminE
                     // ucurrent[i][getGlobalStateIndex(s.getLocalStates())] = exp;//v^k_i(s)=maxminE

                     bybool[i0] = false;//remove byzantine role from i
                     bybool[j0] = false;//remove byzantine role from i
                     vprev[i0] = vcurrent[i0];
                     vprev[j0] = vcurrent[j0];


                 }
             });
                double[] bigDelta = new double[n];
                double ep1, ep2;
                int countfail = 0;
                int countpass = 0;
                double distance = 0;
                double minxy = 20;

                bigDelta[ratlist[0]] = vcurrent[ratlist[0]][getGlobalStateIndex(gstates[0].getLocalStates())] - ucurrent[ratlist[0]][getGlobalStateIndex(gstates[0].getLocalStates())];
                bigDelta[ratlist[1]] = vcurrent[ratlist[1]][getGlobalStateIndex(gstates[0].getLocalStates())] - ucurrent[ratlist[1]][getGlobalStateIndex(gstates[0].getLocalStates())];
                double epi1, epi2, epj1, epj2;
                epi1 = bigDelta[ratlist[0]] - 2 * EiK;
                epi2 = bigDelta[ratlist[0]] + 2 * EiK;
                epj1 = bigDelta[ratlist[1]] - 2 * EiK;
                epj2 = bigDelta[ratlist[1]] + 2 * EiK;


                if ((epi1 >= 0 && epj1 > eps) || (epj1 >= 0 && epi1 > eps))
                {
                    return "unstable";
                }
                else if (epi2 <= eps && epj2 <= eps)
                {
                    return "stable with eps" + eps;
                }
                else if (epi1 <= eps && epj1 <= eps)
                {


                    return "stable with epsdel" + (eps + delta);//if (eps <= epi2 || eps <= epj2)

                }
                else
                {
                    distance = Math.Sqrt(Math.Pow(epi2 - eps, 2) + Math.Pow(epj2 - eps, 2));
                    if (distance < min_distance)
                    {
                        min_distance = distance;
                        deltastar = Math.Min(Math.Abs(epi2 - eps), Math.Abs(epj2 - eps));
                    }
                    result = "undecided";
                }
               
                alpha += 0.2;
                if (alpha >= 1)
                {
                    return "NEepsdelta*" + " " + deltastar;
                }



            }


            // System.Diagnostics.Debug.WriteLine(result);
            return result;


        }
        private static void setglobalAction(int i0, int a_i, GlobalAction a_z, GlobalAction a_al)
        {
            int bizcount = 0;
            int alcount = 0;
            List<int> byz = a_z.getLocalActions();//as a list of local actions
            List<int> al = a_al.getLocalActions();//as a list of local actions

            //only the size matters
            for (int i = 0; i < n; i++)
            {
                if (i == i0)
                {
                    Actions[i] = a_i;//not taken from any list

                }
                else if (bybool[i])
                {
                    Actions[i] = byz[bizcount];
                    bizcount++;
                }
                else
                {
                    Actions[i] = al[alcount];
                    alcount++;
                }
            }
        }

        private static void setglobalAction(int i0, int a_i,  GlobalAction a_al)
        {
          //  int bizcount = 0;
            int alcount = 0;
          //  List<int> byz = a_z.getLocalActions();//as a list of local actions
            List<int> al = a_al.getLocalActions();//as a list of local actions

            //only the size matters
            for (int i = 0; i < n; i++)
            {
                if (i == i0)
                {
                    Actions[i] = a_i;//not taken from any list

                }
                else
                {
                    Actions[i] = al[alcount];
                    alcount++;
                }
            }
        }

        private static void setglobalAction(int i0, int j0, GlobalAction a_ij, GlobalAction a_z, GlobalAction a_al)
        {
            int bizcount = 0;
            int alcount = 0;
            List<int> byz=new List<int>();
            if(a_z!=null)
                byz = a_z.getLocalActions();//as a list of local actions Z

            List<int> al;
            if (a_al != null)
                al = a_al.getLocalActions();//as a list of local actions Al-ij
            else al = new List<int>();
            for (int i = 0; i < n; i++)
            {
                if (i == i0)
                {
                    Actions[i] = a_ij.getLocalActions()[0];//not taken from any list

                }
                else if (i == j0)
                {
                    Actions[i] = a_ij.getLocalActions()[1];//not taken from any list
                }
                else if (bybool[i])
                {
                    Actions[i] = byz[bizcount];//actions from Z
                    bizcount++;
                }
                else
                {
                    Actions[i] = al[alcount];//actions from Al-ij
                    alcount++;
                }
            }
        }

        private static void computegActions(List<int> abefore, int i, List<GlobalAction> byzAc, int size)
        {
            if (i < size) {
                foreach (int a_i in localactionset[i])
                {
                    List<int> temaction = new List<int>(abefore);// integer list of actions already calculated
                    temaction.Add(a_i);//add the action of the new player
                    // System.Diagnostics.Debug.WriteLine(s_i.getY() + " " + s_i.getZ());
                    if (i + 1 == size)
                    {

                        byzAc.Add(new GlobalAction(temaction));//all the players are considers so add the new action to list


                    }
                    else
                    {
                        computegActions(temaction, i + 1, byzAc, size);
                    }
                }
            
            }
            
        }

        public static int getGlobalStateIndex(State[] localstates)
        {
            int index = 0;
            
            index = localstates[0].getLocalIndex();
            for (int j = 1; j < n; j++)
            {
                index = index * (AState.getNo_of_States()) + localstates[j].getLocalIndex();
            }
            
            return index;
        }
        private static GlobalState BT(GlobalState s, int[] a, List<GlobalState> gstates)
        {
            State[] localstates = new AState[n];
            for (int i = 0; i < n; i++)
            {
                AState aS=(AState)(s.getLocalStates()[i]);
                int label = aS.getLabel();
                int l = aS.getL();
                int[] c = aS.getC();
                int[] aSData=new int[n+2];
                if (label != 2) {
                    int shootcount = 0;
                   
                    for (int j = 0; j <n; j++)
                    {
                        if (j != i && a[j] == (i+1))//convert zero based index of players to one based index for shooting
                        {
                            shootcount++;
                            if(c[j]>0)//lose confidence with shootings
                                c[j]--;
                        }
                        aSData[j]=c[j];
                    }
                    l = Math.Max(0, l - shootcount);
                    label = (label + 1) % 3;
                    aSData[n]=l;
                    aSData[n+1]=label;
                   
                }
                else {
                    for (int j = 0; j < n; j++)
                    {
                        if (j != (i))//update confidence
                        {
                            c[j] = Math.Min(2, c[j] + 1);
                        }
                        aSData[j]=c[j];
                    }
                    l = 2;//restore life
                    label = (label + 1) % 3;
                    aSData[n] = l;
                    aSData[n+1] = label;
                }
                localstates[i] = new AState(i, aSData);
            }

            return gstates[getGlobalStateIndex(localstates)];
           
           
        }

        private static double H(int i, GlobalState s, int[] a)
        {
            double R = 4;
            int lipos = 0;//if li>0
            int aizer = 0;//if ai=0
            double ajzercount = 0;
            AState aS = (AState)s.getLocalStates()[i];
            int li = aS.getL();

            if (li > 0)
                lipos = 1;
            if (a[i] == 0)
                aizer = 1;

            if (lipos * aizer == 1)
            {
                for (int j = 0; j < n; j++)
                {
                    if (a[j] == 0)
                        ajzercount++;
                }
                return R / ajzercount;
            }
            else return 0;
            //int c = -2, r = 2, w = -1;
            //// only valid state action pairs are accepted
            //int z_i = s.getLocalStates()[i].getZ();//get z_i
            //int y_i = s.getLocalStates()[i].getY();//get y_i
            //if (z_i == 0 && a[i] == 1)
            //{
            //    return c;
            //}
            //else if (z_i == 1 && a[i] == 0 && !gamma(i, s))//stay in finished state if the job not completed
            //{
            //    return w;
            //}
            //else if (z_i == 1 && a[i] == 1 && gamma(i, s))//go to completed state if a job completed
            //{
            //    return r;
            //}
            //else
            //{
            //    return 0;
            //}
            return 0;
        }

        private static double Pr(int i, GlobalState s, int[] a)
        {
            AState aS=(AState)(s.getLocalStates()[i]);//retrive the state of player i
            int[] c= aS.getC();//retrive confidence list of player i
            double normal_sum=0;
            double maxcij = 0;
            for (int j = 0; j < n; j++)
            {
                AState aSj=(AState)(s.getLocalStates()[j]);
                if (j != i)
                {
                    if(aSj.getL()>0){//only the living ones are considered for shooting
                        if (c[j] > maxcij)//find max confidence value
                            maxcij = c[j];
                        normal_sum+=c[j];
                    }
                        
                    

                }
             


            }
            if(normal_sum>0){//if some other player is alive
                if (a[i] == 0)
                    return 1 - maxcij / (normal_sum);
                else
                {
                    for (int j = 1; j <= n; j++)
                    {
                        if (a[i] == j)
                        {
                            AState aSj = (AState)(s.getLocalStates()[j-1]);//shift the index of j
                            int lj = aSj.getL();
                            if (lj == 0)
                            {
                                return 0;
                            }
                            else return (maxcij * c[j - 1]) / normal_sum;//shifting the index to zero based
                        }
                    }
                }
            
            }
            else //if no other player is alive
            {
                if (a[i] == 0)
                    return 1;
                else return 0;
            }
            

            return 0;
        }

        private static double GlobalPr(GlobalState s, int[] a)
        {
            double prob = 1;
            for (int i = 0; i < n; i++)
            {
                if (!bybool[i])
                {
                    prob *= Pr(i, s, a);//use the independance of probability
                }
            }
            return prob;
        }

        private static void init(int i,String name)
        {
            
           
                
                
                for (int s = 0; s < gstates.Count; s++)
                {
                    if (name.Equals("vc"))
                    {
                        if (s == 0)
                            vcurrent[i] = new List<double>();
                        vcurrent[i].Add(0);
                    }
                    else if (name.Equals("vp")) {
                        if (s == 0)
                            vprev[i] = new List<double>();
                        vprev[i].Add(0);
                    }
                    else if (name.Equals("uc")) {
                        if (s == 0)
                            ucurrent[i] = new List<double>();
                        ucurrent[i].Add(0);
                    }
                    else
                    {
                        if (s == 0)
                            uprev[i] = new List<double>();
                        uprev[i].Add(0);
                    }
                    
                }
          
        }

       
        private static void computeGstates(List<State> before, int i, List<GlobalState> gstates, List<State>[] locstatelist)
        {

            foreach (State s_i in locstatelist[i])
            {
                List<State> temstate = new List<State>(before);
                temstate.Add(s_i);
                // System.Diagnostics.Debug.WriteLine(s_i.getY() + " " + s_i.getZ());
                if (i + 1 == n)
                {

                    gstates.Add(new GlobalState(temstate));


                }
                else
                {
                    computeGstates(temstate, i + 1, gstates, locstatelist);
                }
            }
        }
    }


    public class GlobalAction
    {
        //consists of a list of combination of actions
        //  const int n = 3;//number of players have to updated
        List<int> localactions = new List<int>();//try to find 
        public GlobalAction(List<int> lactions)//actions related to a set of players
        {
            for (int i = 0; i < lactions.Count; i++)
            {
                this.localactions.Add(lactions[i]);
                // System.Diagnostics.Debug.Write(localstates[i].getY() + " " + localstates[i].getZ() + " ,");
            }
            //System.Diagnostics.Debug.Write(Program.gamma(0, this));
            //System.Diagnostics.Debug.Write('\n');
        }
        public List<int> getLocalActions()
        {
            return this.localactions;
        }

    }
    public class GlobalState
    {
        const int n = 3;//number of players have to updated

        State[] localstates = new State[n];//try to find 
        public GlobalState(List<State> lstates)
        {
            for (int i = 0; i < n; i++)
            {
                this.localstates[i] = lstates[i];
                //    System.Diagnostics.Debug.Write(localstates[i].getY() + " " + localstates[i].getZ()+" ,");
            }
            //    System.Diagnostics.Debug.Write(Program.gamma(0,this));
            //    System.Diagnostics.Debug.Write('\n');
        }
        public State[] getLocalStates()
        {
            return this.localstates;
        }

    }
    public class State
    {
        int player_id;
        int local_index = 0;
        
        public State(int player_id)
        {
            this.player_id = player_id;
        }
        public virtual int getLocalIndex()
        {
            return 0;
        }

     //   public static virtual void AddStates(List<State> slist, int player_id) { }

    }

    public class AState : State {

       private int l,label;
        const int n = 3;
       private static int no_of_states = 0;
       private int[] c = new int[n];
        public AState(int player_id,int[] c)
            : base(player_id)

        {
            for (int j = 0; j < n; j++)
            {
                this.c[j] = c[j];
            }
            this.l = c[n];
            this.label = c[n+1];
        }

        public static int getNo_of_States()
        {
            return AState.no_of_states;
        }
        public static new void AddStates(List<State> slist,int player_id) {
            //create all the possible states to a list
            int[] con = new int[n+2];//con is the array of values for one state instance
            addstrec(con, 0,slist,player_id);
            AState.no_of_states = slist.Count;
        }

        private static void addstrec(int[] con,  int p,List<State> slist,int player_id)
        {
            if (p < n + 2) {
                for (int i = 0; i < 3; i++)
                {
                    con[p] = i;
                   addstrec(con, p + 1,slist,player_id);
                }
            }
            else
            {
                State s=new AState(player_id,con);
                slist.Add(s);
            }
           
        }

        public new int[] getC()
        {
            return this.c;
        }

        public new int getLabel()
        {
            return this.label;
        }

        public new int getL()
        {
            return this.l;
        }

        public override int getLocalIndex()
        {
           
            int sum = c[0];
            for (int j = 1; j <n; j++)
            {
                sum = sum * 3 + c[j];
            }
            return sum*9+l* 3 + label;//base 3 number
        }
        
    }
    public class JState : State
    {
        int y;
        int z;
       
        public JState(int player_id, int y, int z)
            : base(player_id)
        {
            this.y = y;
            this.z = z;
        }

        public int getY()
        {
            return y;
        }
        public int getZ()
        {
            return z;
        }

        public override int getLocalIndex()
        {
            return y * 3 + z;
        }
        public void setY(int y)
        {
            this.y = y;
        }
        public void setZ(int z)
        {
            this.z = z;
        }
    }
}




