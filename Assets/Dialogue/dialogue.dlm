STATE s_questItem DEFAULT (bool)false
STATE s_hasItem DEFAULT (bool)false

test_npc:
    IF !s_questItem {
        c_test \ Hi, could you get me the item?
        CHOICE {
            c_player * Yes
        accept_quest:
            c_test \ Okay, thanks.
            RUN s_questItem = true
            HALT

            c_player * No
            c_test \ Please?
            CHOICE {
                c_player * Fine
                GOTO accept_quest

                c_player * Still no
                c_test \ Shame...
                HALT
            }

            c_player * Maybe later
            c_test \ I'll wait.
            HALT
        }
    } ELSE {
        IF s_hasItem {
            c_test \ Thank you for the item.
            HALT
        } ELSE {
            c_test \ I'm waiting for the item.
            HALT
        }
    }
