import React, {Component} from 'react';
import {RoomHubEvents} from '../RoomConstants';
import styles from './chat.module.css';

export default class Chat extends Component {
    constructor(props) {
        super(props);
        this.state = {
            messages: [],
            currentMessage: ''
        };
    }

    componentDidMount() {
        this.props.hubConnection.on(RoomHubEvents.NEW_MESSAGE, (from, text) => {
            this.setState({
                messages: [{from, text}, ...this.state.messages]
            })
        });

        this.props.hubConnection.on(RoomHubEvents.NOTIFY, (notification) => {
            this.setState({
                messages: [{from: notification, text: ''}, ...this.state.messages]
            })
        });
    }

    onPostMessage = (event) => {
        event.preventDefault();
        this.props.hubConnection.invoke(RoomHubEvents.SEND, this.props.roomId, this.props.user.name, this.state.currentMessage);
        this.setState({currentMessage: ''})
    };

    onChangeInput = (event) => {
        const {value} = event.target;

        this.setState({
            ...this.state,
            currentMessage: value
        });
    };

    render() {
        return (
            <section className={styles.chat}>
                <header className={`btn-sm ${styles.title}`}>Чат</header>
                <section className={styles.messages}>
                    {this.state.messages.map(message =>
                        <p className={styles.message}>
                            <span className={styles.from}>{message.from}</span> <span className={styles.text}>{message.text}</span>
                        </p>
                    )}
                </section>
                <form className={styles.form} onSubmit={this.onPostMessage}>
                    <input
                        className={styles.input}
                        value={this.state.currentMessage}
                        placeholder="Отвечать тут..."
                        type='text'
                        onChange={this.onChangeInput}
                    />
                </form>

            </section>
        )
    }
}
