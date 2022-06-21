from typing import Dict
from argparse import ArgumentParser
from pdb import set_trace as stop

import numpy as np
import tensorflow as tf
tf.compat.v1.disable_eager_execution()
import time, random, os
from unityagents import UnityEnvironment
import matplotlib.pyplot as plt
from mlagents_envs.environment import UnityEnvironment

import optuna
import numpy as np
import mlflow

def sample_hyper_parameters(
    trial: optuna.trial.Trial,
    force_linear_model: bool = False,
) -> Dict:


    actor_learning_rate = trial.suggest_loguniform("actor_learning_rate", 1e-5, 1e-2)
    #critic_learning_rate = trial.suggest_loguniform("critic_learning_rate", 1e-5, 1e-2)
    #tau = trial.suggest_loguniform("tau", 1e-3, 0.2)
    #variance = trial.suggest_categorical("variance", [3, 4, 5, 6])
    #gamma = trial.suggest_loguniform("gamma", 0.7, 1.2)
    #add buffer_size and batch_size
    #batch_size = trial.suggest_categorical("batch_size", [16, 32, 64, 128])
    #memory_size = trial.suggest_categorical("memory_size", [int(1e4), int(5e4), int(1e5)])

    return {
        'actor_learning_rate': actor_learning_rate,
        #'critic_learning_rate': critic_learning_rate,
        #'tau': tau,
    }


def objective(
    trial,
    force_linear_model,
    n_episodes_to_train,
    env ,
):
    
    
    env.reset()
    
    brain_name = list(env.behavior_specs)[0]
    brain = env.behavior_specs[brain_name]
    decision_steps, terminal_steps = env.get_steps(brain_name)

    num_agents = len(decision_steps)
    print("num agents ", num_agents)

    if brain.action_spec.discrete_size > 0:
        for action, branch_size in enumerate(brain.action_spec.discrete_branches):
            print(f"Action number {action} has {branch_size} different options")
            state_size = branch_size

    action_size = brain.action_spec.continuous_size
    print("action size ", action_size)

    decision_steps, terminal_steps = env.get_steps(brain_name)
    state_size = len(decision_steps.obs[0][0])
    print("state size ", state_size)

    print("Agent state looks like: \n{}".format(decision_steps.obs[0]))

    with mlflow.start_run():

        # hyper-parameters
        args = sample_hyper_parameters(trial,
                                       force_linear_model=force_linear_model)
        mlflow.log_params(trial.params)
        
        print("PARAMS: actor_learning_rate = ", args['actor_learning_rate'])

        # create agent object
        agent = DDPGAgent(state_size= state_size - 3, action_size=action_size, goal_size=3, action_high=1,
                          action_low=-1, actor_learning_rate = args['actor_learning_rate'], critic_learning_rate= 1e-3,
                          tau = 0.1)
        

        # train loop
        rewards = train(agent, env,
              n_episodes=n_episodes_to_train)
        
        mean_reward = np.mean(rewards)
        std_reward = np.std(rewards)
        mlflow.log_metric('mean_reward', mean_reward)
        mlflow.log_metric('std_reward', std_reward)
        print("mlflow loop, mean_reward = ", mean_reward, "std_reward = ", std_reward)

    return mean_reward

def train(
    agent,
    env,
    n_episodes: int,
) -> []:

    variance=5
    # Tensorborad log writer
    logging = False
    use_her = True # use hindsight experience replay or not
    num_episodes = 20 # number of episodes over which success rate is computed
    episode_length = 500
    optimization_steps = 40
    K = 8 # number of random future states

    a_losses = []
    c_losses = []
    ep_mean_r = []
    success_rate = []

    ep_experience = Episode_experience()
    ep_experience_her = Episode_experience()

    total_step = 0
    for i in range(n_episodes):
        successes = 0
        ep_total_r = 0
        for n in range(num_episodes):
            
            env.reset()
            brain_name = list(env.behavior_specs)[0]
            brain = env.behavior_specs[brain_name]
            decision_steps, terminal_steps = env.get_steps(brain_name)
            
            state = decision_steps.obs[0][0][3:]
            goal = decision_steps.obs[0][0][:3]
            
            for ep_step in range(episode_length):
                total_step += 1
                action = agent.choose_action([state], [goal], variance)
                
                env_action = brain.action_spec.random_action(len(decision_steps))
                for j in range(len(action)):
                    env_action.continuous[0][j] = action[j]
                
                env.set_actions(brain_name, env_action)
                env.step()
                decision_steps, terminal_steps = env.get_steps(brain_name)
                
                for agent_id_decisions in decision_steps:
                    next_state = decision_steps.obs[0][0][3:]
                for agent_id_terminated in terminal_steps:
                    next_state = terminal_steps.obs[0][0][3:]
                
                for agent_id_decisions in decision_steps:
                    reward = decision_steps.reward[0]
                for agent_id_terminated in terminal_steps:
                    reward = terminal_steps.reward[0]
                
                dones = terminal_steps.interrupted
                done=False
                if (len(dones) > 0):
                    done = True
                else:
                    done=False         
                
                ep_total_r += reward
                ep_experience.add(state, action, reward, next_state, done, goal)
                state = next_state
                if total_step % 200 == 0 or done:
                    if use_her: # The strategy can be changed here
                        for t in range(len(ep_experience.memory)):
                            for _ in range(K):
                                future = np.random.randint(t, len(ep_experience.memory))
                                goal_ = ep_experience.memory[future][3][3:6] # next_state of future
                                state_ = ep_experience.memory[t][0]
                                action_ = ep_experience.memory[t][1]
                                next_state_ = ep_experience.memory[t][3]
                                done_, reward_ = reward_func(next_state_, goal_)
                                ep_experience_her.add(state_, action_, reward_, next_state_, done_, goal_)
                        agent.remember(ep_experience_her)
                        ep_experience_her.clear()
                    agent.remember(ep_experience)
                    ep_experience.clear()
                    variance *= 0.9995
                    a_loss, c_loss = agent.replay(optimization_steps)
                    a_losses += [a_loss]
                    c_losses += [c_loss]
                    agent.update_target_net()
                if done:
                    break
            successes += reward>=0 and done

        success_rate.append(successes/num_episodes)
        ep_mean_r.append(ep_total_r/num_episodes)
        print("\repoch", i+1, "success rate %.2f"%success_rate[-1], "ep_mean_r %.2f"%ep_mean_r[-1], 'exploration %.2f'%variance, end=' '*10)

    return ep_mean_r

def reward_func(state, goal):
    hand = state[3:6]
    dist = np.linalg.norm(hand-goal)*5 # range=5
    done = False
    reward = -1
    if dist<=1:
        done = True
        reward = 1
    return done, reward
    
class Episode_experience():
    def __init__(self):
        self.memory = []

    def add(self, state, action, reward, next_state, done, goal):
        self.memory += [(state, action, reward, next_state, done, goal)]

    def clear(self):
        self.memory = []

class DDPGAgent:
    def __init__(self, state_size, action_size, goal_size, action_low=-1, action_high=1, gamma=0.98,
                 actor_learning_rate=0.01, critic_learning_rate=0.01, tau=1e-3):
        self.state_size = state_size
        self.action_size = action_size
        self.goal_size = goal_size
        self.action_low = action_low
        self.action_high = action_high
        self.gamma = gamma   # discount rate
        self.memory = []
        self.buffer_size = int(5e4)
        self.a_learning_rate = actor_learning_rate
        self.c_learning_rate = critic_learning_rate # often larger than actor_learning_rate
        self.tau = tau # soft update
        self.batch_size = 32
        self.gradient_norm_clip = None
        self._construct_nets()

    def _construct_nets(self):
        tf.compat.v1.reset_default_graph()
        self.sess = tf.compat.v1.Session()

        self.S = tf.compat.v1.placeholder(tf.float32, [None, self.state_size], 'state')
        self.S_ = tf.compat.v1.placeholder(tf.float32, [None, self.state_size], 'next_state')
        self.G = tf.compat.v1.placeholder(tf.float32, [None, self.goal_size], 'goal')
        self.D = tf.compat.v1.placeholder(tf.float32, [None, 1], 'done')
        self.R = tf.compat.v1.placeholder(tf.float32, [None, 1], 'r')

        with tf.compat.v1.variable_scope('Actor'):
            self.a = self._build_a(self.S, self.G, scope='eval')
            self.a_ = self._build_a(self.S_, self.G, scope='target')
        with tf.compat.v1.variable_scope('Critic'):
            self.q = self._build_c(self.S, self.a, self.G, scope='eval')
            self.q_ = self._build_c(self.S_, self.a_, self.G, scope='target')

        self.ae_params = tf.compat.v1.get_collection(tf.compat.v1.GraphKeys.GLOBAL_VARIABLES, scope='Actor/eval')
        self.at_params = tf.compat.v1.get_collection(tf.compat.v1.GraphKeys.GLOBAL_VARIABLES, scope='Actor/target')
        self.ce_params = tf.compat.v1.get_collection(tf.compat.v1.GraphKeys.GLOBAL_VARIABLES, scope='Critic/eval')
        self.ct_params = tf.compat.v1.get_collection(tf.compat.v1.GraphKeys.GLOBAL_VARIABLES, scope='Critic/target')

        self.soft_update_op = [[tf.compat.v1.assign(ta, (1 - self.tau) * ta + self.tau * ea), 
                                tf.compat.v1.assign(tc, (1 - self.tau) * tc + self.tau * ec)]
                    for ta, ea, tc, ec in zip(self.at_params, self.ae_params, self.ct_params, self.ce_params)]

        q_target = self.R + self.gamma * (1-self.D) * self.q_
#         q_target = tf.clip_by_value(q_target, -1/(1-self.gamma), 0)

        self.c_loss = tf.compat.v1.losses.mean_squared_error(q_target, self.q)
        self.a_loss = - tf.reduce_mean(input_tensor=self.q)    # maximize the q

        if self.gradient_norm_clip is not None:
            c_optimizer = tf.compat.v1.train.AdamOptimizer(self.c_learning_rate)
            c_gradients = c_optimizer.compute_gradients(self.c_loss, var_list=self.ce_params)
            for i, (grad, var) in enumerate(c_gradients):
                if grad is not None:
                    c_gradients[i] = (tf.clip_by_norm(grad, self.gradient_norm_clip), var)
            self.c_train = c_optimizer.apply_gradients(c_gradients)
            a_optimizer = tf.compat.v1.train.AdamOptimizer(self.a_learning_rate)
            a_gradients = c_optimizer.compute_gradients(self.a_loss, var_list=self.ae_params)
            for i, (grad, var) in enumerate(a_gradients):
                if grad is not None:
                    a_gradients[i] = (tf.clip_by_norm(grad, self.gradient_norm_clip), var)
            self.a_train = a_optimizer.apply_gradients(a_gradients)
        else:
            self.c_train = tf.compat.v1.train.AdamOptimizer(self.c_learning_rate).minimize(self.c_loss, var_list=self.ce_params)
            self.a_train = tf.compat.v1.train.AdamOptimizer(self.a_learning_rate).minimize(self.a_loss, var_list=self.ae_params)

        self.saver = tf.compat.v1.train.Saver()
        self.sess.run(tf.compat.v1.global_variables_initializer())

    def _build_a(self, s, g, scope): # policy
        with tf.compat.v1.variable_scope(scope):
            net = tf.concat([s, g], 1)
            net = tf.compat.v1.layers.dense(net, 64, tf.nn.relu)
            net = tf.compat.v1.layers.dense(net, 64, tf.nn.relu)
            net = tf.compat.v1.layers.dense(net, 64, tf.nn.relu)
            a = tf.compat.v1.layers.dense(net, self.action_size, tf.nn.tanh)
            result = a * (self.action_high-self.action_low)/2 + (self.action_high+self.action_low)/2
            return result

    def _build_c(self, s, a, g, scope): # Q value
        with tf.compat.v1.variable_scope(scope):
            net = tf.concat([s, a, g], 1)
            net = tf.compat.v1.layers.dense(net, 64, tf.nn.relu)
            net = tf.compat.v1.layers.dense(net, 64, tf.nn.relu)
            net = tf.compat.v1.layers.dense(net, 64, tf.nn.relu)
            return tf.compat.v1.layers.dense(net, 1)

    def choose_action(self, state, goal, variance): # normal distribution
        action = self.sess.run(self.a, {self.S: state, self.G: goal})[0]
        return np.clip(np.random.normal(action, variance), self.action_low, self.action_high)

    def remember(self, ep_experience):
        self.memory += ep_experience.memory
        if len(self.memory) > self.buffer_size:
            self.memory = self.memory[-self.buffer_size:] # empty the first memories

    def replay(self, optimization_steps=1):
        if len(self.memory) < self.batch_size: # if there's no enough transitions, do nothing
            return 0, 0

        a_losses = 0
        c_losses = 0
        for _ in range(optimization_steps):
            minibatch = np.vstack(random.sample(self.memory, self.batch_size))
            ss = np.vstack(minibatch[:,0])
            acs = np.vstack(minibatch[:,1])
            rs = np.vstack(minibatch[:,2])
            nss = np.vstack(minibatch[:,3])
            ds = np.vstack(minibatch[:,4])
            gs = np.vstack(minibatch[:,5])
            a_loss, _ = self.sess.run([self.a_loss, self.a_train],
                                      {self.S: ss, self.G: gs})
            c_loss, _ = self.sess.run([self.c_loss, self.c_train],
                                      {self.S: ss, self.a: acs, self.R: rs,
                                       self.S_: nss, self.D: ds, self.G: gs})
            a_losses += a_loss
            c_losses += c_loss

        return a_losses/optimization_steps, c_losses/optimization_steps

    def update_target_net(self):
        self.sess.run(self.soft_update_op)


if __name__ == '__main__':

    parser = ArgumentParser()
    parser.add_argument('--trials', type=int, required=True)
    parser.add_argument('--episodes', type=int, required=True)
    parser.add_argument('--force_linear_model', dest='force_linear_model', action='store_true')
    parser.set_defaults(force_linear_model=False)
    parser.add_argument('--experiment_name', type=str, required=True)
    args = parser.parse_args()

    # set Mlflow experiment name
    mlflow.set_experiment(args.experiment_name)

    # set Optuna study
    study = optuna.create_study(study_name=args.experiment_name,
                                direction='maximize',
                                load_if_exists=True,
                                storage=f'sqlite:///{OPTUNA_DB}')

    # Wrap the objective inside a lambda and call objective inside it
    # Nice trick taken from https://www.kaggle.com/general/261870
    func = lambda trial: objective(trial, force_linear_model=args.force_linear_model, n_episodes_to_train=args.episodes)

    # run Optuna
    study.optimize(func, n_trials=args.trials)