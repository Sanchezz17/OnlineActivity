import React, {Component} from 'react';
import {RoomHubEvents} from "../RoomConstants";
import styles from './leaderboard.module.css';

export default class Leaderboard extends Component {
    constructor(props) {
        super(props);
        this.state = {
            players: [],
            explainingPlayer: null,
            loadingPlayers: true
        };
    }

    async componentDidMount() {
        this.props.hubConnection.on(RoomHubEvents.NEW_PLAYER, (player) => {
            this.setState({
                players: [...this.state.players, player]
            });
        });

        this.props.hubConnection.on(RoomHubEvents.LEAVE_ROOM, (name) => {
            this.setState({
                players: this.state.players.filter(p => p.name !== name)
            });
        });

        this.props.hubConnection.on(RoomHubEvents.NEW_ROUND, (explainingPlayer) => {
            this.setState({
                explainingPlayer: explainingPlayer
            });
            
            if (explainingPlayer.name === this.props.user.name) {
                // toDo что если объясняющий это я?
            }
        });

        await this.fetchPlayers();
    }

    fetchPlayers = async () => {
        const response = await fetch(`/api/players?roomId=${this.props.roomId}`);
        const players = await response.json();
        this.setState({
            loadingPlayers: false,
            players
        });
    };

    renderPlayers = () => {
        return this.state.players
            .map(player =>
                <div className={styles.player} key={player.name}>
                    {player.photoUrl && <img className={styles.player__image} src={player.photoUrl} alt={player.name}/>}
                    <span className={styles.player__name}>{player.name}</span>
                    <span className={styles.player__score}>{player.score}</span>
                </div>
            )
            .sort(p => p.score);
    }

    render() {
        return (
            <section className={styles.board}>
                <div className={styles.players}>
                    {(this.state.loadingPlayers)
                        ? <div className={styles.loading}>
                            <p>Загрузка игроков...</p>
                        </div>
                        : this.renderPlayers()
                    }
                </div>
            </section>
        );
    }
}
