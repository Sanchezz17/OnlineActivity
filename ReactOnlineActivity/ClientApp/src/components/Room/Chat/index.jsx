import React, { Component } from 'react';
import { RoomHubEvents } from '../RoomConstants';
import styles from './chat.module.css';
import { MessageState } from './MessageState';

export default class Chat extends Component {
    constructor(props) {
        super(props);
        this.state = {
            messages: [],
            currentMessageText: '',
            isActiveUser: false
        };
    }

    componentDidMount() {
        this.props.hubConnection.on(RoomHubEvents.NEW_MESSAGE, this.addMessage);

        this.props.hubConnection.on(RoomHubEvents.ROUND_INFO, (explainingPlayerName) => {
            this.setState({ isActiveUser: explainingPlayerName === this.props.user.name });
        });
        
        this.props.hubConnection.on(RoomHubEvents.MESSAGE_RATED, (message) => {
            const index = this.state.messages.findIndex(m => m.id === message.id);
            this.state.messages[index].state = message.state;
            this.setState({ messages: this.state.messages });
        });

        this.props.hubConnection.on(RoomHubEvents.GAME_OVER, (winner) => {
            this.addMessage(winner);
            setTimeout(() => {
                this.setState({
                    messages: [],
                    currentMessageText: '',
                    isActiveUser: false
                });
                
            }, 10000);
        });
    }
    
    addMessage = (message) => {
        this.setState({
            messages: [
                message,
                ...this.state.messages
            ]
        });
    };

    onPostMessage = (event) => {
        event.preventDefault();
        if (this.state.currentMessageText === '')
            return;
        this.props.hubConnection.invoke(
            RoomHubEvents.SEND, 
            this.props.roomId, 
            {
                id: null,
                from: this.props.user.name,
                text: this.state.currentMessageText,
                state: MessageState.NotRated
            });
        this.setState({ currentMessageText: '' });
    };

    onChangeCurrentMessageText = (event) => {
        const { value } = event.target;

        this.setState({
            currentMessageText: value
        });
    };
    
    rateMessage = (message, value) => {
        message.state = value ? MessageState.Like : MessageState.Dislike;
        this.props.hubConnection.invoke(RoomHubEvents.MESSAGE_RATED, this.props.roomId, message);
    }

    render() {
        return (
            <section className={styles.chat}>
                <header className={`btn-sm ${styles.title}`}>Чат</header>
                <section className={styles.messages}>
                    {this.state.messages.map((message, index) =>
                        <p className={styles.message}>
                            <span className={styles.from}>{message.from}</span> <span className={styles.text}>{message.text}</span>
                            {message.text && 
                            <>
                                <input 
                                    type="radio"
                                    name={`mark-${index}`} 
                                    id={`switching-like-${index}`}
                                    className={styles.switchingLikeView}
                                    disabled={!this.state.isActiveUser}
                                    checked={message.state === MessageState.Like}
                                    onChange={() => this.rateMessage(message, true)}
                                />
                                <label 
                                    htmlFor={`switching-like-${index}`} 
                                    className={styles.switchingLikeView__label}
                                />
                                <input 
                                    type="radio" 
                                    name={`mark-${index}`}
                                    id={`switching-dislike-${index}`} 
                                    className={styles.switchingDislikeView}
                                    disabled={!this.state.isActiveUser}
                                    checked={message.state === MessageState.Dislike}
                                    onChange={() => this.rateMessage(message, false)}
                                />
                                <label 
                                    htmlFor={`switching-dislike-${index}`}
                                    className={styles.switchingDislikeView__label}
                                />
                            </>}
                        </p>
                    )}
                </section>
                <form className={styles.form} onSubmit={this.onPostMessage}>
                    <input
                        className={styles.input}
                        value={this.state.currentMessageText}
                        placeholder="Отвечать тут..."
                        type='text'
                        onChange={this.onChangeCurrentMessageText}
                        disabled={this.state.isActiveUser}
                    />
                </form>
            </section>
        );
    }
}
