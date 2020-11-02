import random as rand
from os import system
from collections import defaultdict

class BoardEnvironment:

    def __init__(self):
        "init board"

    def set_players(self, AI):
        self.AI = AI
        self.reset()

    def reset(self):
        self.turn = 'X'

        self.board = list('---------')
        if(rand.random() < 0.5):
            self.current_player = True
        else:
            self.current_player = False

        return self.current_player

    def print_board(self, board_string = None):
        if not board_string:
            B = self.board
        else:
            B = board_string
        check_for = ['X', 'O']
        print(B[0] if B[0] in check_for else 1,'|', B[1] if B[1] in check_for else 2,'|', B[2] if B[2] in check_for else 3, sep='')
        print('-----')
        print(B[3] if B[3] in check_for else 4,'|', B[4] if B[4] in check_for else 5,'|', B[5] if B[5] in check_for else 6, sep='')
        print('-----')
        print(B[6] if B[6] in check_for else 7,'|', B[7] if B[7] in check_for else 8,'|', B[8] if B[8] in check_for else 9, sep='')

    def get_state(self):
        return "".join(self.board)

    def other_player(self):
        return not self.current_player

    def available_actions(self, first):
        return [ind for ind, val in enumerate(self.board) if val == '-']

    def play_game(self):
        self.reset()
        while( not self.is_full() ):
            system('clear')
            if( not self.current_player ):
                choice = self.AI.select_action(None)
            else:
                self.print_board()
                choices = self.available_actions(None)
                print("Select your space to play. Your pieces are", self.turn + '.', "Current choices are")
                print(list(x+1 for x in choices))
                choice = 10
                while(choice not in choices):
                    choice = input()
                    choice = int(choice) - 1
                    if(choice not in choices):
                        print("Spot not available. Current choices are")
                        print(list(x+1 for x in choices))

            self.board[choice] = self.turn

            if self.winner(self.turn):
                system('clear')
                if(self.current_player):
                    print("You won!")
                else:
                    print("You lost!")
                self.print_board()
                return self.current_player

            self.turn = 'X' if self.turn == 'O' else 'O'
            self.current_player = not self.current_player
        system('clear')
        self.print_board()
        print("Tie!")

        return None

    def winner(self, check_for = ['X', 'O']):
        straight_lines = ((0,1,2),(3,4,5),(6,7,8),(0,3,6),
                          (1,4,7),(2,5,8),(0,4,8),(2,4,6))
        for turn in check_for:
            for line in straight_lines:
                if all(x == turn for x in (self.board[i] for i in line)):
                    return turn
        return ''

    def is_full(self):
        return('-' not in self.board)

class Agent:
    def __init__(self, environment, difficulty, policy = 'max'):
        self.environment = environment
        self.policy = policy
        self.Q = ''
        if policy == 'max':
            with open(difficulty, 'r') as f:
                for i in f.readlines():
                    self.Q = i
            self.Q = eval(self.Q)
        self.Q = defaultdict(lambda: 0.0, self.Q)
        self.reset_past()

    def reset_past(self):
        self.past_action = None
        self.past_state = None

    def select_action(self, first):
        available_actions = self.environment.available_actions(first)
        if(self.policy == 'random'):
            choice = rand.choice(available_actions)
        else:
            Q_vals = [self.Q[(self.environment.get_state(), x)] for x in available_actions]
            max_val = max(Q_vals)
            max_pos = [i for i, j in enumerate(Q_vals) if j == max_val]
            max_indices = [available_actions[x] for x in max_pos]
            choice = rand.choice(max_indices)
        self.past_state = self.environment.get_state()
        self.past_action = choice
        return choice

class LeagueEnvironment:
    def __init__(self, board_environment):
        self.board = board_environment    

    def set_players(self, player_names, league_agents, board_agents):
        self.player_names = player_names
        self.league_agents = league_agents
        self.board_agents = board_agents
        assert(len(player_names) == len(league_agents) == len(board_agents) )
        self.num_players = len(player_names)

    def reset_pair(self):
        # randomly select 2 players
        player_indices = list(range(self.num_players))
        self.Ai = rand.choice(player_indices)
        self.board.set_players(self.board_agents[self.Ai])
        self.first = self.board.reset()
        self.league_agents[self.Ai].reset_past()
        self.A_wins = 0;
        self.A_chips=100;
        self.Player_wins = 0;
        self.Player_chips=100;
        self.ties = 0;
        self.state_perspective = 'A' # the state in wins/ties/losses for which player
        self.chip_mul=1
        self.min_bid=5
        self.game_counter=1

    def get_state(self):  ### how to tell who is calling get_state?
        return (self.A_chips,self.A_wins,self.ties,self.Player_chips,self.Player_wins,self.player_names[self.Ai],'learning strategy and tactics')

    def pair_games_played(self):
        return self.A_wins + self.ties + self.B_wins

    def available_actions(self, first):
        if first:
            return ['quit','single bet','double bet','triple bet']
        else:
            return ['quit','call']

    def play_pair(self):
        system('clear')
        self.reset_pair()

        player_choice = ''

        while(True):

            if self.first:
                player_choice = self.league_choice(True)
                AI_choice = self.league_agents[self.Ai].select_action(False)
                print("Opponent chose", AI_choice)
            else:
                AI_choice = self.league_agents[self.Ai].select_action(True)
                player_choice = self.league_choice(False, AI_choice)

            if AI_choice == 'quit' or player_choice == 'quit':
                break
            elif AI_choice == 'single bet' or player_choice == 'single bet':
                self.chip_mul=1
            elif AI_choice == 'double bet' or player_choice == 'double bet':
                self.chip_mul=2
            elif AI_choice == 'triple bet' or player_choice == 'triple bet':
                self.chip_mul=3

            winner = self.board.play_game()
            self.first = not self.first

            if winner == True:
                print("Player wins!")
                self.Player_wins += 1
                self.Player_chips += self.min_bid*self.chip_mul
                self.A_chips -= self.min_bid*self.chip_mul
            elif winner == False:
                print("Ai wins!")
                self.A_wins += 1
                self.A_chips += self.min_bid*self.chip_mul
                self.Player_chips -= self.min_bid*self.chip_mul
            else:
                self.ties += 1

            if self.A_chips <= 0 or self.Player_chips <= 0:
                break

        if player_choice == 'quit' or self.Player_chips <= 0:
            print("Player is no longer playing")

        else:
            print("Play again? 1 for yes, 0 for no")
            again = -1
            while again < 0 or again > 1:
                again = int(input())
            if again == 1:
                self.play_pair()
            return


    def league_choice(self, first, AI_choice = ''):
        choice_list = self.available_actions(first)
        i = 0
        p_input = -1
        print("You currently have", self.Player_chips, "chips and", self.Player_wins, "wins.")
        if AI_choice:
            print("Opponent chose", AI_choice)
        print('Select a choice from the list:')
        for choice in choice_list:
            print(i, choice)
            i += 1
        while p_input < 0 or p_input > len(choice_list):
            p_input = int(input())
        return choice_list[p_input]


def select_difficulty(select = False):
    x = 0
    diffdict = {1 : r'easy.txt',
                2 : r'medium.txt',
                3 : r'hard.txt'}
    if select:
        while(x > 3 or x < 1):
            print("Select a difficulty:")
            print("1: Easy")
            print("2: Medium")
            print("3: Hard")
            x = int(input())
    
    else:
        x = rand.randint(1, 3)

    return diffdict[x]

#system('clear')
board = BoardEnvironment()
league = LeagueEnvironment(board)

player_names = []
board_agents = []
league_agents = []

player_names.append('learning strategy and tactics')
board_agents.append(Agent(board, select_difficulty(), 'max'))
league_agents.append(Agent(league, 'league.txt', 'max'))

player_names.append('learning tactics only')
board_agents.append(Agent(board, select_difficulty(), 'max'))
league_agents.append(Agent(league, 'league.txt', 'random'))

player_names.append('learning strategy only')
board_agents.append(Agent(board, select_difficulty(), 'random'))
league_agents.append(Agent(league, 'league.txt', 'max'))

player_names.append('no learning')
board_agents.append(Agent(board, select_difficulty(), 'random'))
league_agents.append(Agent(league, 'league.txt', 'random'))

league.set_players(player_names, league_agents, board_agents)
league.play_pair()

#board.set_players(AI)
#board.play_game()
