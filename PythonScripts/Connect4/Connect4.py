import random
import copy
import numpy as np
import time
from collections import defaultdict

class BoardEnvironment:
  """ this class creates an environment for agents to interact with"""

  def __init__(self):
    "initialize board"
    return

  def set_players(self, playerA):
    " connects players with the environment "
    self.playerA = playerA
    self.reset() # defines current_player

  def reset(self):
    self.turn = 'X' # the board always starts with X, regardless of which player

    # board states are a 42-character representing the state of the board.
    self.board = list('-------------------------')
    if (self.playerA): # if they are set
      self.playerA.reset_past()
      if (random.random() < 0.5):  # randomly pick the player to start
        self.current_player = True
      else:
        self.current_player = False

  def print_board(self, board_string = None):
    "print more readable board either from supplied board string or the current board"
    if not board_string:
      B = self.board
    else:
      B = board_string
    print(B[0],'|', B[1],'|', B[2],'|',B[3],'|',B[4], sep='')
    # print('-------------')
    print(B[5],'|', B[6],'|', B[7],'|',B[8],'|',B[9], sep='')
    # print('-------------')
    print(B[10],'|', B[11],'|', B[12],'|',B[13],'|',B[14], sep='')
    # print('-------------')
    print(B[15],'|', B[16],'|', B[17],'|',B[18],'|',B[19], sep='')
    # print('-------------')
    print(B[20],'|', B[21],'|', B[22],'|',B[23],'|',B[24], sep='')

  def get_state(self):
    return "".join(self.board)

  def print_options(self):
    temp_board = copy.copy(self.board)
    for col in range(5):
      bottom = self.get_lowest_column(col)
      if(bottom != -1):
        temp_board[bottom] = col + 1
    self.print_board(temp_board)

  def get_lowest_column(self, i):
    if(self.board[i] == '-'):
      while(i + 5 < 25):
        if(self.board[i+5] == '-'):
          i = i + 5
        else:
          break
    else:
      return -1
    return i

  def other_player(self):
    # note, returns other player even if playerA is playing itself
    return not self.current_player

  def available_actions(self):
    movelist = []
    for i in range(5):
        if self.board[i] == '-':
            movelist.append(i)
        else:
            continue
    return movelist

  def play_game(self):
    # returns the winning player or None if a tie
    self.reset()
    while (not self.is_full() ):

      # ************ HUMAN-PLAYABLE MODIFICATION
      if(self.current_player):
        choice = self.playerA.select_action(self.board)
      else:
        print("Your board piece is ", self.turn)
        print("Select a board piece from the options below:")
        movelist = self.available_actions()
        self.print_options()
        x = int(input())
        while(x < 1 or x > 5 or (x-1) not in movelist):
            print("Invalid choice. Please select another board piece.")
            x = int(input())
        print()
        choice = self.get_lowest_column(int(x) - 1)
      # *********************************************




      self.board[choice] = self.turn # should check if valid

      if self.winner(self.turn):
        self.print_board()
        print(self.turn, "won!")
        return self.current_player

      # switch players
      self.turn = 'X' if self.turn == 'O' else 'O' # switch turn
      self.current_player = self.other_player()
    # it's a tie
    return None

  def winner(self, turn):
    straight_lines = (
	    (0,1,2,3),
	    (1,2,3,4),
            (5,6,7,8),
            (6,7,8,9),
            (10,11,12,13),
            (11,12,13,14),
            (15,16,17,18),
            (16,17,18,19),
            (20,21,22,23),
            (21,22,23,24),

            (0,6,12,18),
            (6,12,18,24),
            (1,7,13,19),
            (5,11,17,23),


            (3,7,11,15),
            (4,8,12,16),
            (8,12,16,20),
            (9,13,17,21),

	    (0,5,10,15),
            (5,10,15,20),
            (1,6,11,16),
            (6,11,16,21),
            (2,7,12,17),
            (7,12,17,22),
            (3,8,13,18),
            (8,13,18,23),
            (4,9,14,19),
            (9,14,19,24)
			)
#     for turn in check_for:
    for line in straight_lines:
        if all(x == turn for x in (self.board[i] for i in line)):
            return turn
    return '' # if there is no winner

  def is_full(self):
    return('-' not in self.board)

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

    def select_action(self, states):
      print("AI select")
      available_actions = self.environment.available_actions()
      Q_vals = [self.Q[(self.environment.get_state(), x)] for x in available_actions]
      #randomly pick one of the maximum values
      max_val = max(Q_vals) # will often be 0 in the beginning
      max_pos = [i for i, j in enumerate(Q_vals) if j == max_val]
      max_indices = [available_actions[x] for x in max_pos]
      choice = random.choice(max_indices)
      self.past_state = self.environment.get_state()
      self.past_action = choice

      while(choice + 5 < 25):
          if(self.environment.board[choice+5] == '-'):
              choice = choice + 5
          else:
              break

      return choice

def select_difficulty():
    x = 0
    diffdict = {1: r'easy.txt',
                2: r'medium.txt',
                3: r'hard.txt'}
    while(x > 3 or x < 1):
        print("Select a difficulty:")
        print("1: Easy")
        print("2: Medium")
        print("3: Hard")
        x = int(input())

    return diffdict[x]

board = BoardEnvironment()
A = Agent(board, select_difficulty())
board.set_players(A)
board.play_game()
