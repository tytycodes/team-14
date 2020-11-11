import random as rand
from collections import defaultdict
class Agent:
  """ this class is a generic Q-Learning reinforcement learning agent for discrete states and fixed actions
  represented as strings"""
  def __init__(self, environment, difficulty):
    self.environment = environment
    tempdict = ''
    with open(difficulty, 'r') as f:
      for i in f.readlines():
        tempdict = i
    tempdict = eval(tempdict)
    self.Q = defaultdict(lambda: 0.0, tempdict)
    self.reset_past()
  def reset_past(self):
    self.past_action = None
    self.past_state = None
  def select_action(self):
    available_actions = self.environment.available_actions()
    Q_vals = [self.Q[(self.environment.get_state(), x)] for x in available_actions]
    #randomly pick one of the maximum values
    max_val = max(Q_vals) # will often be 0 in the beginning
    max_pos = [i for i, j in enumerate(Q_vals) if j == max_val]
    max_indices = [available_actions[x] for x in max_pos]
    choice = rand.choice(max_indices)
    self.past_state = self.environment.get_state()
    self.past_action = choice
		#only do this on the board level
    while(choice + 5 < 25):
      if(self.environment.board[choice+5] == '-'):
        choice = choice + 5
      else:
        break
    return choice